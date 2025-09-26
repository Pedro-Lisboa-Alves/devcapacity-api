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

public class TasksServiceTests
{
    private static CreateUpdateTasksDto NewCreateDto(
        string name = "TaskA",
        int initiative = 1,
        int status = 1,
        int pds = 5,
        DateTime? start = null,
        DateTime? end = null) =>
        new()
        {
            Name = name,
            Initiative = initiative,
            Status = status,
            PDs = pds,
            StartDate = start ?? DateTime.Today,
            EndDate = end ?? DateTime.Today.AddDays(7)
        };

    private static Tasks ToEntity(int id,
        string name = "TaskA",
        int initiative = 1,
        int status = 1,
        int pds = 5,
        List<EngineerAssignment>? assignments = null,
        DateTime? start = null,
        DateTime? end = null) =>
        new()
        {
            TaskId = id,
            Name = name,
            Initiative = initiative,
            Status = status,
            PDs = pds,
            StartDate = start ?? DateTime.Today,
            EndDate = end ?? DateTime.Today.AddDays(7),
            Assignments = assignments ?? new List<EngineerAssignment>()
        };

    [Fact]
    public void Create_WithUniqueName_CallsRepoAddAndReturnsDto()
    {
        var repo = new Mock<ITasksRepository>();
        repo.Setup(r => r.GetByName(It.IsAny<string>())).Returns((Tasks?)null);
        repo.Setup(r => r.Add(It.IsAny<Tasks>()))
            .Returns<Tasks>(t => { t.TaskId = 1; return t; });

        var svc = new TasksService(repo.Object);

        var dto = NewCreateDto(name: "UniqueTask", initiative: 2, status: 3, pds: 8);
        var created = svc.Create(dto);

        Assert.NotNull(created);
        Assert.Equal(1, created.TaskId);
        Assert.Equal("UniqueTask", created.Name);
        Assert.Equal(2, created.Initiative);
        Assert.Equal(3, created.Status);
        Assert.Equal(8, created.PDs);
        repo.Verify(r => r.Add(It.Is<Tasks>(t => t.Name == "UniqueTask" && t.Initiative == 2)), Times.Once);
    }

    [Fact]
    public void Create_WithDuplicateName_ThrowsInvalidOperationException()
    {
        var repo = new Mock<ITasksRepository>();
        repo.Setup(r => r.GetByName("Existing")).Returns(new Tasks { TaskId = 2, Name = "Existing" });

        var svc = new TasksService(repo.Object);

        var dto = NewCreateDto(name: "Existing");

        Assert.Throws<InvalidOperationException>(() => svc.Create(dto));
    }

    [Fact]
    public void GetAll_ReturnsMappedDtos()
    {
        var items = new List<Tasks>
        {
            ToEntity(1, "A", assignments: new List<EngineerAssignment> { new EngineerAssignment { AssignmentId = 11 } }),
            ToEntity(2, "B", assignments: new List<EngineerAssignment> { new EngineerAssignment { AssignmentId = 22 } })
        };

        var repo = new Mock<ITasksRepository>();
        repo.Setup(r => r.GetAll()).Returns(items);

        var svc = new TasksService(repo.Object);

        var result = svc.GetAll().ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.TaskId == 1 && d.Name == "A" && d.AssignmentIds.Contains(11));
        Assert.Contains(result, d => d.TaskId == 2 && d.Name == "B" && d.AssignmentIds.Contains(22));
    }

    [Fact]
    public void GetById_ReturnsDto_WhenFound()
    {
        var repo = new Mock<ITasksRepository>();
        repo.Setup(r => r.GetById(3)).Returns(ToEntity(3, "C", assignments: new List<EngineerAssignment> { new EngineerAssignment { AssignmentId = 7 } }));

        var svc = new TasksService(repo.Object);

        var dto = svc.GetById(3);

        Assert.NotNull(dto);
        Assert.Equal(3, dto!.TaskId);
        Assert.Equal("C", dto.Name);
        Assert.Contains(7, dto.AssignmentIds);
    }

    [Fact]
    public void Update_NonExisting_ReturnsFalse()
    {
        var repo = new Mock<ITasksRepository>();
        repo.Setup(r => r.GetById(99)).Returns((Tasks?)null);

        var svc = new TasksService(repo.Object);

        var result = svc.Update(99, NewCreateDto("Someone"));
        Assert.False(result);
    }

    [Fact]
    public void Update_ChangingToDuplicateName_ThrowsInvalidOperationException()
    {
        var repo = new Mock<ITasksRepository>();
        // existing target
        repo.Setup(r => r.GetById(1)).Returns(new Tasks { TaskId = 1, Name = "Orig" });
        // another task with the new name
        repo.Setup(r => r.GetByName("Dup")).Returns(new Tasks { TaskId = 2, Name = "Dup" });

        var svc = new TasksService(repo.Object);

        var dto = NewCreateDto(name: "Dup");

        Assert.Throws<InvalidOperationException>(() => svc.Update(1, dto));
    }

    [Fact]
    public void Delete_NonExisting_ReturnsFalse_And_Existing_ReturnsTrue()
    {
        var repo = new Mock<ITasksRepository>();
        repo.Setup(r => r.Delete(10)).Returns(false);
        repo.Setup(r => r.Delete(2)).Returns(true);

        var svc = new TasksService(repo.Object);

        Assert.False(svc.Delete(10));
        Assert.True(svc.Delete(2));
        repo.Verify(r => r.Delete(2), Times.Once);
    }
}