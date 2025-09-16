using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using DevCapacityApi.Services;
using DevCapacityApi.Repositories;
using DevCapacityApi.DTOs;
using DevCapacityApi.Models;

namespace DevCapacityApi.Tests;

public class TeamServiceTests
{
    private static CreateUpdateTeamDto NewDto(string name = "TeamA", int? parent = null) =>
        new() { Name = name, ParentTeam = parent };

    private static Team ToEntity(int id, string name = "TeamA", int? parent = null, List<Engineer>? engineers = null) =>
        new() { TeamId = id, Name = name, ParentTeam = parent, Engineers = engineers ?? new List<Engineer>() };

    [Fact]
    public void Create_WithUniqueName_CallsRepoAddAndReturnsDto()
    {
        var repo = new Mock<ITeamRepository>();
        repo.Setup(r => r.GetByName(It.IsAny<string>())).Returns((Team?)null);
        repo.Setup(r => r.Add(It.IsAny<Team>()))
            .Returns<Team>(t => { t.TeamId = 1; return t; });

        var svc = new TeamService(repo.Object);

        var dto = NewDto("UniqueTeam");
        var created = svc.Create(dto);

        Assert.NotNull(created);
        Assert.Equal(1, created.TeamId);
        Assert.Equal("UniqueTeam", created.Name);
        repo.Verify(r => r.Add(It.Is<Team>(t => t.Name == "UniqueTeam")), Times.Once);
    }

    [Fact]
    public void Create_WithDuplicateName_ThrowsInvalidOperationException()
    {
        var repo = new Mock<ITeamRepository>();
        repo.Setup(r => r.GetByName("TeamA")).Returns(new Team { TeamId = 2, Name = "TeamA" });

        var svc = new TeamService(repo.Object);

        var dto = NewDto("TeamA");

        Assert.Throws<InvalidOperationException>(() => svc.Create(dto));
    }

    [Fact]
    public void Create_WithParentMissing_ThrowsInvalidOperationException()
    {
        var repo = new Mock<ITeamRepository>();
        repo.Setup(r => r.GetByName(It.IsAny<string>())).Returns((Team?)null);
        repo.Setup(r => r.GetById(99)).Returns((Team?)null);

        var svc = new TeamService(repo.Object);

        var dto = NewDto("ChildTeam", parent: 99);

        Assert.Throws<InvalidOperationException>(() => svc.Create(dto));
    }

    [Fact]
    public void Update_NonExisting_ReturnsFalse()
    {
        var repo = new Mock<ITeamRepository>();
        repo.Setup(r => r.GetById(5)).Returns((Team?)null);

        var svc = new TeamService(repo.Object);

        var result = svc.Update(5, NewDto("Someone"));
        Assert.False(result);
    }

    [Fact]
    public void Update_ChangingToDuplicateName_ThrowsInvalidOperationException()
    {
        var repo = new Mock<ITeamRepository>();
        // existing target
        repo.Setup(r => r.GetById(1)).Returns(new Team { TeamId = 1, Name = "Orig" });
        // another team with the new name
        repo.Setup(r => r.GetByName("Dup")).Returns(new Team { TeamId = 2, Name = "Dup" });

        var svc = new TeamService(repo.Object);

        var dto = NewDto("Dup");

        Assert.Throws<InvalidOperationException>(() => svc.Update(1, dto));
    }

    [Fact]
    public void Update_SetParentToSelf_ThrowsInvalidOperationException()
    {
        var repo = new Mock<ITeamRepository>();
        repo.Setup(r => r.GetById(3)).Returns(new Team { TeamId = 3, Name = "Self" });

        var svc = new TeamService(repo.Object);

        var dto = NewDto("Self", parent: 3);

        Assert.Throws<InvalidOperationException>(() => svc.Update(3, dto));
    }

    [Fact]
    public void Delete_NonExisting_ReturnsFalse_And_Existing_ReturnsTrue()
    {
        var repo = new Mock<ITeamRepository>();
        repo.Setup(r => r.Delete(10)).Returns(false);
        repo.Setup(r => r.Delete(2)).Returns(true);

        var svc = new TeamService(repo.Object);

        Assert.False(svc.Delete(10));
        Assert.True(svc.Delete(2));
        repo.Verify(r => r.Delete(2), Times.Once);
    }

    [Fact]
    public void GetAll_ReturnsMappedDtos()
    {
        var items = new List<Team>
        {
            ToEntity(1, "A", null, new List<Engineer> { new Engineer { Id = 11, Name = "E1" } }),
            ToEntity(2, "B", null, new List<Engineer> { new Engineer { Id = 22, Name = "E2" } })
        };

        var repo = new Mock<ITeamRepository>();
        repo.Setup(r => r.GetAll()).Returns(items);

        var svc = new TeamService(repo.Object);

        var result = svc.GetAll().ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.TeamId == 1 && d.Name == "A" && d.EngineerIds.Contains(11));
        Assert.Contains(result, d => d.TeamId == 2 && d.Name == "B" && d.EngineerIds.Contains(22));
    }

    [Fact]
    public void GetById_ReturnsDto_WhenFound()
    {
        var repo = new Mock<ITeamRepository>();
        repo.Setup(r => r.GetById(3)).Returns(ToEntity(3, "C", null, new List<Engineer> { new Engineer { Id = 7, Name = "E7" } }));

        var svc = new TeamService(repo.Object);

        var dto = svc.GetById(3);

        Assert.NotNull(dto);
        Assert.Equal(3, dto!.TeamId);
        Assert.Equal("C", dto.Name);
        Assert.Contains(7, dto.EngineerIds);
    }
}