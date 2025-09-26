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

public class EngineerAssignmentServiceTests
{
    private static CreateUpdateEngineerAssignmentDto NewCreateDto(int engineerId = 1, int taskId = 100, int capacity = 50) =>
        new() { EngineerId = engineerId, TaskId = taskId, CapacityShare = capacity, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(7) };

    private static EngineerAssignment ToEntity(int id, int engineerId = 1, int taskId = 100, int capacity = 50) =>
        new() { AssignmentId = id, EngineerId = engineerId, TaskId = taskId, CapacityShare = capacity, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(7) };

    [Fact]
    public void Create_WithExistingEngineer_CallsRepoAddAndReturnsDto()
    {
        var repo = new Mock<IEngineerAssignmentRepository>();
        var engRepo = new Mock<IEngineerRepository>();

        engRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns(new Engineer { Id = 1, Name = "E1" });
        repo.Setup(r => r.Add(It.IsAny<EngineerAssignment>()))
            .Returns<EngineerAssignment>(a => { a.AssignmentId = 10; return a; });

        var svc = new EngineerAssignmentService(repo.Object, engRepo.Object);

        var dto = NewCreateDto(engineerId: 1);
        var created = svc.Create(dto);

        Assert.NotNull(created);
        Assert.Equal(10, created.AssignmentId);
        Assert.Equal(1, created.EngineerId);
        repo.Verify(r => r.Add(It.Is<EngineerAssignment>(a => a.EngineerId == 1 && a.TaskId == dto.TaskId)), Times.Once);
    }

    [Fact]
    public void Create_WithMissingEngineer_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IEngineerAssignmentRepository>();
        var engRepo = new Mock<IEngineerRepository>();

        engRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns((Engineer?)null);

        var svc = new EngineerAssignmentService(repo.Object, engRepo.Object);

        var dto = NewCreateDto(engineerId: 99);

        Assert.Throws<InvalidOperationException>(() => svc.Create(dto));
    }

    [Fact]
    public void GetAll_ReturnsMappedDtos()
    {
        var items = new List<EngineerAssignment>
        {
            ToEntity(1, engineerId: 1, taskId: 10),
            ToEntity(2, engineerId: 2, taskId: 20)
        };

        var repo = new Mock<IEngineerAssignmentRepository>();
        var engRepo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.GetAll()).Returns(items);

        var svc = new EngineerAssignmentService(repo.Object, engRepo.Object);

        var result = svc.GetAll().ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.AssignmentId == 1 && d.EngineerId == 1 && d.TaskId == 10);
        Assert.Contains(result, d => d.AssignmentId == 2 && d.EngineerId == 2 && d.TaskId == 20);
    }

    [Fact]
    public void GetById_ReturnsDto_WhenFound()
    {
        var repo = new Mock<IEngineerAssignmentRepository>();
        var engRepo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.GetById(5)).Returns(ToEntity(5, engineerId: 3, taskId: 55));

        var svc = new EngineerAssignmentService(repo.Object, engRepo.Object);

        var dto = svc.GetById(5);

        Assert.NotNull(dto);
        Assert.Equal(5, dto!.AssignmentId);
        Assert.Equal(3, dto.EngineerId);
        Assert.Equal(55, dto.TaskId);
    }

    [Fact]
    public void GetByEngineerId_ReturnsList()
    {
        var items = new List<EngineerAssignment>
        {
            ToEntity(1, engineerId: 7, taskId: 10),
            ToEntity(2, engineerId: 7, taskId: 11)
        };

        var repo = new Mock<IEngineerAssignmentRepository>();
        var engRepo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.GetByEngineerId(7)).Returns(items);

        var svc = new EngineerAssignmentService(repo.Object, engRepo.Object);

        var result = svc.GetByEngineerId(7).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(7, r.EngineerId));
    }

    [Fact]
    public void Update_NonExisting_ReturnsFalse()
    {
        var repo = new Mock<IEngineerAssignmentRepository>();
        var engRepo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.GetById(99)).Returns((EngineerAssignment?)null);

        var svc = new EngineerAssignmentService(repo.Object, engRepo.Object);

        var result = svc.Update(99, NewCreateDto(1));
        Assert.False(result);
    }

    [Fact]
    public void Update_WithMissingEngineer_ThrowsInvalidOperationException()
    {
        var repo = new Mock<IEngineerAssignmentRepository>();
        var engRepo = new Mock<IEngineerRepository>();

        repo.Setup(r => r.GetById(2)).Returns(ToEntity(2, engineerId: 1));
        engRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns((Engineer?)null);

        var svc = new EngineerAssignmentService(repo.Object, engRepo.Object);

        Assert.Throws<InvalidOperationException>(() => svc.Update(2, NewCreateDto(engineerId: 999)));
    }

    [Fact]
    public void Update_Success_ReturnsTrue()
    {
        var repo = new Mock<IEngineerAssignmentRepository>();
        var engRepo = new Mock<IEngineerRepository>();

        var existing = ToEntity(3, engineerId: 1, taskId: 10, capacity: 30);
        repo.Setup(r => r.GetById(3)).Returns(existing);
        engRepo.Setup(r => r.GetById(2)).Returns(new Engineer { Id = 2, Name = "E2" });
        repo.Setup(r => r.Update(It.IsAny<EngineerAssignment>())).Returns(true);

        var svc = new EngineerAssignmentService(repo.Object, engRepo.Object);

        var dto = NewCreateDto(engineerId: 2, taskId: 99, capacity: 80);
        var ok = svc.Update(3, dto);

        Assert.True(ok);
        repo.Verify(r => r.Update(It.Is<EngineerAssignment>(a => a.AssignmentId == 3 && a.EngineerId == 2 && a.TaskId == 99 && a.CapacityShare == 80)), Times.Once);
    }

    [Fact]
    public void Delete_NonExisting_ReturnsFalse_And_Existing_ReturnsTrue()
    {
        var repo = new Mock<IEngineerAssignmentRepository>();
        var engRepo = new Mock<IEngineerRepository>();
        repo.Setup(r => r.Delete(10)).Returns(false);
        repo.Setup(r => r.Delete(2)).Returns(true);

        var svc = new EngineerAssignmentService(repo.Object, engRepo.Object);

        Assert.False(svc.Delete(10));
        Assert.True(svc.Delete(2));
        repo.Verify(r => r.Delete(2), Times.Once);
    }
}