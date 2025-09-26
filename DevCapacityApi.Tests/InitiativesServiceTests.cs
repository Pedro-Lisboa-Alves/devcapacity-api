using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using DevCapacityApi.Repositories;
using DevCapacityApi.Services;
using DevCapacityApi.DTOs;
using DevCapacityApi.Models;

namespace DevCapacityApi.Tests;

public class InitiativesServiceTests
{
    private static CreateUpdateInitiativesDto NewCreateDto(
        string name = "InitiativeA",
        int? parent = null,
        int status = 1,
        int pds = 5,
        DateTime? start = null,
        DateTime? end = null) =>
        new()
        {
            Name = name,
            ParentInitiative = parent,
            Status = status,
            PDs = pds,
            StartDate = start ?? DateTime.Today,
            EndDate = end ?? DateTime.Today.AddDays(30)
        };

    private static Initiatives ToEntity(int id,
        string name = "InitiativeA",
        int? parent = null,
        int status = 1,
        int pds = 5,
        List<Tasks>? tasks = null,
        DateTime? start = null,
        DateTime? end = null) =>
        new()
        {
            InitiativeId = id,
            Name = name,
            ParentInitiative = parent,
            Status = status,
            PDs = pds,
            StartDate = start ?? DateTime.Today,
            EndDate = end ?? DateTime.Today.AddDays(30),
            Tasks = tasks ?? new List<Tasks>()
        };

    [Fact]
    public void Create_WithUniqueName_CallsRepoAddAndReturnsDto()
    {
        var repo = new Mock<IInitiativesRepository>();
        repo.Setup(r => r.GetByName(It.IsAny<string>())).Returns((Initiatives?)null);
        repo.Setup(r => r.Add(It.IsAny<Initiatives>()))
            .Returns<Initiatives>(i => { i.InitiativeId = 1; return i; });

        var svc = new InitiativesService(repo.Object);

        var dto = NewCreateDto(name: "UniqueInitiative");
        var created = svc.Create(dto);

        Assert.NotNull(created);
        Assert.Equal(1, created.InitiativeId);
        Assert.Equal("UniqueInitiative", created.Name);
        repo.Verify(r => r.Add(It.Is<Initiatives>(i => i.Name == "UniqueInitiative")), Times.Once);
    }

    [Fact]
    public void Create_WithDuplicateName_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IInitiativesRepository>();
        repo.Setup(r => r.GetByName("Existing")).Returns(new Initiatives { InitiativeId = 2, Name = "Existing" });

        var svc = new InitiativesService(repo.Object);

        var dto = NewCreateDto(name: "Existing");

        Assert.Throws<InvalidOperationException>(() => svc.Create(dto));
    }

    [Fact]
    public void Create_WithParentMissing_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IInitiativesRepository>();
        repo.Setup(r => r.GetByName(It.IsAny<string>())).Returns((Initiatives?)null);
        repo.Setup(r => r.GetById(99)).Returns((Initiatives?)null);

        var svc = new InitiativesService(repo.Object);

        var dto = NewCreateDto(name: "ChildInitiative", parent: 99);

        Assert.Throws<InvalidOperationException>(() => svc.Create(dto));
    }

    [Fact]
    public void GetAll_ReturnsMappedDtos()
    {
        var items = new List<Initiatives>
        {
            ToEntity(1, "A", tasks: new List<Tasks>{ new Tasks { TaskId = 11 } }),
            ToEntity(2, "B", tasks: new List<Tasks>{ new Tasks { TaskId = 22 } })
        };

        var repo = new Mock<IInitiativesRepository>();
        repo.Setup(r => r.GetAll()).Returns(items);

        var svc = new InitiativesService(repo.Object);

        var result = svc.GetAll().ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.InitiativeId == 1 && d.Name == "A" && d.TaskIds.Contains(11));
        Assert.Contains(result, d => d.InitiativeId == 2 && d.Name == "B" && d.TaskIds.Contains(22));
    }

    [Fact]
    public void GetById_ReturnsDto_WhenFound()
    {
        var repo = new Mock<IInitiativesRepository>();
        repo.Setup(r => r.GetById(3)).Returns(ToEntity(3, "C", tasks: new List<Tasks>{ new Tasks { TaskId = 7 } }));

        var svc = new InitiativesService(repo.Object);

        var dto = svc.GetById(3);

        Assert.NotNull(dto);
        Assert.Equal(3, dto!.InitiativeId);
        Assert.Equal("C", dto.Name);
        Assert.Contains(7, dto.TaskIds);
    }

    [Fact]
    public void Update_NonExisting_ReturnsFalse()
    {
        var repo = new Mock<IInitiativesRepository>();
        repo.Setup(r => r.GetById(99)).Returns((Initiatives?)null);

        var svc = new InitiativesService(repo.Object);

        var result = svc.Update(99, NewCreateDto("Someone"));
        Assert.False(result);
    }

    [Fact]
    public void Update_ChangingToDuplicateName_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IInitiativesRepository>();
        // existing target
        repo.Setup(r => r.GetById(1)).Returns(new Initiatives { InitiativeId = 1, Name = "Orig" });
        // another initiative with the new name
        repo.Setup(r => r.GetByName("Dup")).Returns(new Initiatives { InitiativeId = 2, Name = "Dup" });

        var svc = new InitiativesService(repo.Object);

        var dto = NewCreateDto(name: "Dup");

        Assert.Throws<InvalidOperationException>(() => svc.Update(1, dto));
    }

    [Fact]
    public void Update_SetParentToSelf_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IInitiativesRepository>();
        repo.Setup(r => r.GetById(3)).Returns(new Initiatives { InitiativeId = 3, Name = "Self" });

        var svc = new InitiativesService(repo.Object);

        var dto = NewCreateDto(name: "Self", parent: 3);

        Assert.Throws<InvalidOperationException>(() => svc.Update(3, dto));
    }

    [Fact]
    public void Delete_NonExisting_ReturnsFalse_And_Existing_ReturnsTrue()
    {
        var repo = new Mock<IInitiativesRepository>();
        repo.Setup(r => r.Delete(10)).Returns(false);
        repo.Setup(r => r.Delete(2)).Returns(true);

        var svc = new InitiativesService(repo.Object);

        Assert.False(svc.Delete(10));
        Assert.True(svc.Delete(2));
        repo.Verify(r => r.Delete(2), Times.Once);
    }
}
