using ContactCatalog.Models;
using ContactCatalog.Repositories;
using ContactCatalog.Services;
using ContactCatalog.Validators;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ContactCatalog.Tests;

public class ContactServiceTests
{
    private readonly Mock<IContactRepository> _mockRepo;
    private readonly Mock<ILogger<ContactService>> _mockLogger;
    private readonly ContactService _service;

    public ContactServiceTests()
    {
        _mockRepo = new Mock<IContactRepository>();
        _mockLogger = new Mock<ILogger<ContactService>>();
        _service = new ContactService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public void AddContact_ValidData_CallsRepositoryAdd()
    {
        // Arrange
        var name = "Yasin Ahmadi";
        var email = "yasin@example.com";
        var tags = new List<string> { "work" };

        // Act
        _service.AddContact(name, email, tags);

        // Assert
        _mockRepo.Verify(r => r.Add(It.Is<Contact>(c => 
            c.Name == name && 
            c.Email == email && 
            c.Tags.Count == 1 && 
            c.Tags[0] == "work"
        )), Times.Once);
    }

    [Fact]
    public void AddContact_InvalidData_ThrowsException()
    {
        // Arrange
        var name = "";
        var email = "yasin@example.com";
        var tags = new List<string>();

        _mockRepo.Setup(r => r.Add(It.IsAny<Contact>()))
            .Throws(new InvalidContactException("Name cannot be empty"));

        // Act & Assert
        Assert.Throws<InvalidContactException>(() => 
            _service.AddContact(name, email, tags));
    }

    [Fact]
    public void AddContact_DuplicateEmail_LogsWarningAndThrows()
    {
        // Arrange
        var name = "Yasin Ahmadi";
        var email = "duplicate@example.com";
        var tags = new List<string>();

        _mockRepo.Setup(r => r.Add(It.IsAny<Contact>()))
            .Throws(new InvalidContactException($"Email {email} already exists"));

        // Act & Assert
        Assert.Throws<InvalidContactException>(() => 
            _service.AddContact(name, email, tags));

        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Duplicate email attempt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ListContacts_ReturnsAllContacts()
    {
        // Arrange
        var testContacts = new List<Contact>
        {
            new Contact { Id = 1, Name = "Yasin Ahmadi", Email = "yasin@example.com", Tags = new List<string> { "work" } },
            new Contact { Id = 2, Name = "Jane Smith", Email = "jane@example.com", Tags = new List<string> { "friend" } }
        };

        _mockRepo.Setup(r => r.GetAll()).Returns(testContacts);

        // Act
        var result = _service.ListContacts();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.Name == "Yasin Ahmadi");
        Assert.Contains(result, c => c.Name == "Jane Smith");
    }

    [Fact]
    public void SearchContacts_WithQuery_ReturnsFilteredResults()
    {
        // Arrange
        var query = "Yasin";
        var testContacts = new List<Contact>
        {
            new Contact { Id = 1, Name = "Yasin Ahmadi", Email = "yasin@example.com", Tags = new List<string>() },
            new Contact { Id = 2, Name = "Johnny Walker", Email = "johnny@example.com", Tags = new List<string>() }
        };

        _mockRepo.Setup(r => r.SearchByName(query))
            .Returns(testContacts);

        // Act
        var result = _service.SearchContacts(query);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Contains("Yasin", c.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SearchContacts_NoMatches_ReturnsEmpty()
    {
        // Arrange
        var query = "NonExistent";
        _mockRepo.Setup(r => r.SearchByName(query))
            .Returns(Enumerable.Empty<Contact>());

        // Act
        var result = _service.SearchContacts(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FilterByTag_WithTag_ReturnsOnlyMatchingContacts()
    {
        // Arrange
        var tag = "work";
        var testContacts = new List<Contact>
        {
            new Contact { Id = 1, Name = "Alice", Email = "alice@example.com", Tags = new List<string> { "work", "friend" } },
            new Contact { Id = 2, Name = "Bob", Email = "bob@example.com", Tags = new List<string> { "friend" } },
            new Contact { Id = 3, Name = "Charlie", Email = "charlie@example.com", Tags = new List<string> { "work" } }
        };

        _mockRepo.Setup(r => r.FilterByTag(tag))
            .Returns(testContacts
                .Where(c => c.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                .OrderBy(c => c.Name));

        // Act
        var result = _service.FilterByTag(tag);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Contains(tag, c.Tags));
        
        // Verify sorted by name
        var resultList = result.ToList();
        Assert.Equal("Alice", resultList[0].Name);
        Assert.Equal("Charlie", resultList[1].Name);
    }

    [Fact]
    public void FilterByTag_NoMatches_ReturnsEmpty()
    {
        // Arrange
        var tag = "nonexistent";
        _mockRepo.Setup(r => r.FilterByTag(tag))
            .Returns(Enumerable.Empty<Contact>());

        // Act
        var result = _service.FilterByTag(tag);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void AddContact_Success_LogsInformation()
    {
        // Arrange
        var name = "Yasin Ahmadi";
        var email = "yasin@example.com";
        var tags = new List<string> { "work" };

        // Act
        _service.AddContact(name, email, tags);

        // Assert - Verify information log was called
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Contact added")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

