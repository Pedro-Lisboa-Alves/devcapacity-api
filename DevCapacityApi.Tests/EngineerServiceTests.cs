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

public class EngineerServiceTests
{
    private static EngineerDto NewDto(string name = "Alice") =>
        new() { Name = name, Role = "Dev", DailyCapacity = 8 };

    private static Engineer ToEntity(EngineerDto d) =>
        new() { Id = d.Id, Name = d.Name, Role = d.Role, DailyCapacity = d.DailyCapacity };

    [Fact]
    public void Create_WithUniqueName_CallsRepoAddAndReturnsDto()
    {
        var repo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.GetByName(It.IsAny<string>())).Returns((Engineer?)null);
        repo.Setup(r => r.Add(It.IsAny<Engineer>()))
            .Returns<Engineer>(e => { e.Id = 1; return e; });

        var svc = new EngineerService(repo.Object);

        var dto = NewDto("UniqueName");
        var created = svc.Create(dto);

        Assert.NotNull(created);
        Assert.Equal(1, created.Id);
        Assert.Equal("UniqueName", created.Name);
        repo.Verify(r => r.Add(It.Is<Engineer>(e => e.Name == "UniqueName")), Times.Once);
    }

    [Fact]
    public void Create_WithDuplicateName_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.GetByName("Alice")).Returns(new Engineer { Id = 2, Name = "Alice" });

        var svc = new EngineerService(repo.Object);

        var dto = NewDto("Alice");

        Assert.Throws<InvalidOperationException>(() => svc.Create(dto));
    }

    [Fact]
    public void Update_NonExisting_ReturnsFalse()
    {
        var repo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.GetById(99)).Returns((Engineer?)null);

        var svc = new EngineerService(repo.Object);

        var result = svc.Update(99, NewDto("Someone"));
        Assert.False(result);
    }

    [Fact]
    public void Update_ChangingToDuplicateName_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IEngineerRepository>();
        // existing target
        repo.Setup(r => r.GetById(1)).Returns(new Engineer { Id = 1, Name = "Orig" });
        // another engineer with the new name
        repo.Setup(r => r.GetByName("Bob")).Returns(new Engineer { Id = 2, Name = "Bob" });

        var svc = new EngineerService(repo.Object);

        var dto = NewDto("Bob");

        Assert.Throws<InvalidOperationException>(() => svc.Update(1, dto));
    }

    [Fact]
    public void Delete_NonExisting_ReturnsFalse_And_Existing_ReturnsTrue()
    {
        var repo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.GetById(10)).Returns((Engineer?)null);
        repo.Setup(r => r.GetById(2)).Returns(new Engineer { Id = 2, Name = "E" });

        var svc = new EngineerService(repo.Object);

        Assert.False(svc.Delete(10));
        Assert.True(svc.Delete(2));
        repo.Verify(r => r.Delete(2), Times.Once);
    }

    [Fact]
    public void GetAll_ReturnsMappedDtos()
    {
        var items = new List<Engineer>
        {
            new Engineer { Id = 1, Name = "A", Role = "X", DailyCapacity = 5 },
            new Engineer { Id = 2, Name = "B", Role = "Y", DailyCapacity = 6 }
        };

        var repo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.GetAll()).Returns(items);

        var svc = new EngineerService(repo.Object);

        var result = svc.GetAll().ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.Id == 1 && d.Name == "A" && d.DailyCapacity == 5);
        Assert.Contains(result, d => d.Id == 2 && d.Name == "B" && d.DailyCapacity == 6);
    }

    [Fact]
    public void GetById_ReturnsDto_WhenFound()
    {
        var repo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.GetById(3)).Returns(new Engineer { Id = 3, Name = "C", Role = "R", DailyCapacity = 7 });

        var svc = new EngineerService(repo.Object);

        var dto = svc.GetById(3);

        Assert.NotNull(dto);
        Assert.Equal(3, dto!.Id);
        Assert.Equal("C", dto.Name);
    }
}