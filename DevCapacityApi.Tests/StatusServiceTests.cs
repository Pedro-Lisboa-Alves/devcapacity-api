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

public class StatusServiceTests
{
    private static CreateUpdateStatusDto NewCreateDto(string name = "Open") =>
        new() { Name = name };

    private static Status ToEntity(int id, string name = "Open") =>
        new() { StatusId = id, Name = name };

    [Fact]
    public void Create_WithUniqueName_CallsRepoAddAndReturnsDto()
    {
        var repo = new Mock<IStatusRepository>();
        repo.Setup(r => r.GetByName(It.IsAny<string>())).Returns((Status?)null);
        repo.Setup(r => r.Add(It.IsAny<Status>()))
            .Returns<Status>(s => { s.StatusId = 1; return s; });

        var svc = new StatusService(repo.Object);

        var dto = NewCreateDto("UniqueStatus");
        var created = svc.Create(dto);

        Assert.NotNull(created);
        Assert.Equal(1, created.StatusId);
        Assert.Equal("UniqueStatus", created.Name);
        repo.Verify(r => r.Add(It.Is<Status>(s => s.Name == "UniqueStatus")), Times.Once);
    }

    [Fact]
    public void Create_WithDuplicateName_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IStatusRepository>();
        repo.Setup(r => r.GetByName("Existing")).Returns(new Status { StatusId = 2, Name = "Existing" });

        var svc = new StatusService(repo.Object);

        var dto = NewCreateDto("Existing");

        Assert.Throws<InvalidOperationException>(() => svc.Create(dto));
    }

    [Fact]
    public void GetAll_ReturnsMappedDtos()
    {
        var items = new List<Status>
        {
            ToEntity(1, "A"),
            ToEntity(2, "B")
        };

        var repo = new Mock<IStatusRepository>();
        repo.Setup(r => r.GetAll()).Returns(items);

        var svc = new StatusService(repo.Object);

        var result = svc.GetAll().ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.StatusId == 1 && d.Name == "A");
        Assert.Contains(result, d => d.StatusId == 2 && d.Name == "B");
    }

    [Fact]
    public void GetById_ReturnsDto_WhenFound()
    {
        var repo = new Mock<IStatusRepository>();
        repo.Setup(r => r.GetById(3)).Returns(ToEntity(3, "C"));

        var svc = new StatusService(repo.Object);

        var dto = svc.GetById(3);

        Assert.NotNull(dto);
        Assert.Equal(3, dto!.StatusId);
        Assert.Equal("C", dto.Name);
    }

    [Fact]
    public void Update_NonExisting_ReturnsFalse()
    {
        var repo = new Mock<IStatusRepository>();
        repo.Setup(r => r.GetById(99)).Returns((Status?)null);

        var svc = new StatusService(repo.Object);

        var result = svc.Update(99, NewCreateDto("Someone"));
        Assert.False(result);
    }

    [Fact]
    public void Update_ChangingToDuplicateName_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IStatusRepository>();
        // existing target
        repo.Setup(r => r.GetById(1)).Returns(new Status { StatusId = 1, Name = "Orig" });
        // another status with the new name
        repo.Setup(r => r.GetByName("Dup")).Returns(new Status { StatusId = 2, Name = "Dup" });

        var svc = new StatusService(repo.Object);

        var dto = NewCreateDto("Dup");

        Assert.Throws<InvalidOperationException>(() => svc.Update(1, dto));
    }

    [Fact]
    public void Delete_NonExisting_ReturnsFalse_And_Existing_ReturnsTrue()
    {
        var repo = new Mock<IStatusRepository>();
        repo.Setup(r => r.Delete(10)).Returns(false);
        repo.Setup(r => r.Delete(2)).Returns(true);

        var svc = new StatusService(repo.Object);

        Assert.False(svc.Delete(10));
        Assert.True(svc.Delete(2));
        repo.Verify(r => r.Delete(2), Times.Once);
    }
}