using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TodoApi.Controllers;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Repositories;
using Xunit;

namespace TodoApi.Tests.Controllers;

public class TodosControllerTests
{
    private readonly Mock<ITodoRepository> _repositoryMock = new();
    private readonly Mock<ILogger<TodosController>> _loggerMock = new();
    private readonly TodosController _sut;

    public TodosControllerTests()
    {
        _sut = new TodosController(_repositoryMock.Object, _loggerMock.Object);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_WhenRepositoryHasItems_ReturnsOkWithMappedResponses()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "First" },
            new() { Title = "Second" }
        };
        _repositoryMock.Setup(r => r.GetAll()).Returns(items);

        var result = _sut.GetAll();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var responses = ok.Value.Should().BeAssignableTo<IEnumerable<TodoResponse>>().Subject;
        responses.Should().HaveCount(2);
    }

    [Fact]
    public void GetAll_WhenRepositoryIsEmpty_ReturnsOkWithEmptyList()
    {
        _repositoryMock.Setup(r => r.GetAll()).Returns([]);

        var result = _sut.GetAll();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<TodoResponse>>()
            .Which.Should().BeEmpty();
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetById_ExistingId_ReturnsOkWithItem()
    {
        var item = new TodoItem { Title = "Test" };
        _repositoryMock.Setup(r => r.GetById(item.Id)).Returns(item);

        var result = _sut.GetById(item.Id);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<TodoResponse>()
            .Which.Id.Should().Be(item.Id);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNotFound()
    {
        _repositoryMock.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((TodoItem?)null);

        var result = _sut.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidRequest_ReturnsCreatedAtRoute()
    {
        var request = new CreateTodoRequest("Buy groceries");
        _repositoryMock
            .Setup(r => r.Add(It.IsAny<TodoItem>()))
            .Returns((TodoItem item) => item);

        var result = _sut.Create(request);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.Value.Should().BeOfType<TodoResponse>()
            .Which.Title.Should().Be("Buy groceries");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Create_EmptyOrWhitespaceTitle_ReturnsBadRequest(string title)
    {
        var request = new CreateTodoRequest(title);

        var result = _sut.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Create_TrimsWhitespaceFromTitle()
    {
        TodoItem? captured = null;
        _repositoryMock
            .Setup(r => r.Add(It.IsAny<TodoItem>()))
            .Callback<TodoItem>(item => captured = item)
            .Returns((TodoItem item) => item);

        _sut.Create(new CreateTodoRequest("  Walk the dog  "));

        captured.Should().NotBeNull();
        captured!.Title.Should().Be("Walk the dog");
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ExistingId_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Delete(id)).Returns(true);

        var result = _sut.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Delete_NonExistingId_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Delete(id)).Returns(false);

        var result = _sut.Delete(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── Toggle ────────────────────────────────────────────────────────────────

    [Fact]
    public void Toggle_ExistingIncompleteItem_ReturnsItemAsCompleted()
    {
        var item = new TodoItem { Title = "Task", IsCompleted = false };
        _repositoryMock.Setup(r => r.GetById(item.Id)).Returns(item);
        _repositoryMock.Setup(r => r.Update(It.IsAny<TodoItem>()))
            .Returns((TodoItem t) => t);

        var result = _sut.Toggle(item.Id);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<TodoResponse>()
            .Which.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Toggle_ExistingCompletedItem_ReturnsItemAsIncomplete()
    {
        var item = new TodoItem { Title = "Task", IsCompleted = true };
        _repositoryMock.Setup(r => r.GetById(item.Id)).Returns(item);
        _repositoryMock.Setup(r => r.Update(It.IsAny<TodoItem>()))
            .Returns((TodoItem t) => t);

        var result = _sut.Toggle(item.Id);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<TodoResponse>()
            .Which.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Toggle_NonExistingId_ReturnsNotFound()
    {
        _repositoryMock.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((TodoItem?)null);

        var result = _sut.Toggle(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }
}
