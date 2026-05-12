using FluentAssertions;
using TodoApi.Models;
using TodoApi.Repositories;
using Xunit;

namespace TodoApi.Tests.Repositories;

public class InMemoryTodoRepositoryTests
{
    private readonly InMemoryTodoRepository _sut = new();

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_WhenStoreIsEmpty_ReturnsEmptyCollection()
    {
        _sut.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void GetAll_ReturnsItemsOrderedByCreatedAt()
    {
        var first  = new TodoItem { Title = "First" };
        var second = new TodoItem { Title = "Second" };

        // Ensure distinct timestamps
        Thread.Sleep(5);
        _sut.Add(first);
        Thread.Sleep(5);
        _sut.Add(second);

        var result = _sut.GetAll().ToList();

        result[0].Title.Should().Be("First");
        result[1].Title.Should().Be("Second");
    }

    // ── Add ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Add_ValidItem_ReturnsTheSameItem()
    {
        var item = new TodoItem { Title = "Buy milk" };

        var result = _sut.Add(item);

        result.Should().BeEquivalentTo(item);
    }

    [Fact]
    public void Add_ValidItem_ItemIsRetrievableAfterwards()
    {
        var item = new TodoItem { Title = "Buy milk" };
        _sut.Add(item);

        _sut.GetById(item.Id).Should().NotBeNull();
    }

    [Fact]
    public void Add_MultipleItems_AllAreStored()
    {
        _sut.Add(new TodoItem { Title = "A" });
        _sut.Add(new TodoItem { Title = "B" });
        _sut.Add(new TodoItem { Title = "C" });

        _sut.GetAll().Should().HaveCount(3);
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetById_ExistingId_ReturnsCorrectItem()
    {
        var item = new TodoItem { Title = "Test" };
        _sut.Add(item);

        var result = _sut.GetById(item.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
        result.Title.Should().Be("Test");
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        _sut.GetById(Guid.NewGuid()).Should().BeNull();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public void Delete_ExistingId_ReturnsTrue()
    {
        var item = _sut.Add(new TodoItem { Title = "Delete me" });

        _sut.Delete(item.Id).Should().BeTrue();
    }

    [Fact]
    public void Delete_ExistingId_ItemIsNoLongerRetrievable()
    {
        var item = _sut.Add(new TodoItem { Title = "Delete me" });
        _sut.Delete(item.Id);

        _sut.GetById(item.Id).Should().BeNull();
    }

    [Fact]
    public void Delete_NonExistingId_ReturnsFalse()
    {
        _sut.Delete(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void Delete_DoesNotAffectOtherItems()
    {
        var keep   = _sut.Add(new TodoItem { Title = "Keep" });
        var remove = _sut.Add(new TodoItem { Title = "Remove" });

        _sut.Delete(remove.Id);

        _sut.GetById(keep.Id).Should().NotBeNull();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ExistingItem_ReturnsUpdatedItem()
    {
        var item = _sut.Add(new TodoItem { Title = "Original" });
        item.IsCompleted = true;

        var result = _sut.Update(item);

        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Update_NonExistingItem_ReturnsNull()
    {
        var ghost = new TodoItem { Title = "Ghost" };
        _sut.Update(ghost).Should().BeNull();
    }

    // ── Thread safety ─────────────────────────────────────────────────────────

    [Fact]
    public void Add_ConcurrentWrites_AllItemsAreStored()
    {
        const int count = 100;
        var tasks = Enumerable
            .Range(0, count)
            .Select(i => Task.Run(() => _sut.Add(new TodoItem { Title = $"Item {i}" })))
            .ToArray();

        Task.WaitAll(tasks);

        _sut.GetAll().Should().HaveCount(count);
    }
}
