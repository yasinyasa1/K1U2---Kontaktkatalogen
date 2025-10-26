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
    [Fact]
    public void AddContact_DuplicateEmail_ThrowsInvalidContactException()
    {
        // Arrange
        var mockRepo = new Mock<IContactRepository>();

        mockRepo
            .Setup(r => r.Add(It.IsAny<Contact>()))
            .Throws(new InvalidContactException("Email 'test@gmail.com' already exists"));

        var service = new ContactService(
            mockRepo.Object,
            Mock.Of<ILogger<ContactService>>()
        );

        // Act & Assert
        Assert.Throws<InvalidContactException>(() =>
            service.AddContact(
                "Anna",
                "test@gmail.com",
                new List<string> { "friend", "football" }
            ));

        mockRepo.Verify(r => r.Add(It.IsAny<Contact>()), Times.Once);
    }

    [Fact]
    public void AddContact_InvalidEmail_ThrowsInvalidContactException()
    {
        // Arrange
        var mockRepo = new Mock<IContactRepository>();

        mockRepo
            .Setup(r => r.Add(It.IsAny<Contact>()))
            .Throws(new InvalidContactException("Invalid email format"));

        var service = new ContactService(
            mockRepo.Object,
            Mock.Of<ILogger<ContactService>>()
        );

        // Act & Assert
        Assert.Throws<InvalidContactException>(() =>
            service.AddContact(
                "Anna",
                "notanemail",
                new List<string>()
            ));

        mockRepo.Verify(r => r.Add(It.IsAny<Contact>()), Times.Once);
    }

    [Fact]
    public void ListContacts_ReturnsAllContacts()
    {
        // Arrange
        var mockRepo = new Mock<IContactRepository>();

        mockRepo
            .Setup(r => r.GetAll())
            .Returns(new List<Contact>
            {
                new Contact
                {
                    Id = 1,
                    Name = "Anna",
                    Email = "test@gmail.com",
                    Tags = new List<string> { "friend", "football" }
                },
                new Contact
                {
                    Id = 2,
                    Name = "Bella",
                    Email = "bella@gmail.com",
                    Tags = new List<string> { "gym", "bestfriend" }
                }
            });

        var service = new ContactService(
            mockRepo.Object,
            Mock.Of<ILogger<ContactService>>()
        );

        // Act
        var result = service.ListContacts().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == 1 && c.Name == "Anna");
        Assert.Contains(result, c => c.Id == 2 && c.Name == "Bella");

        mockRepo.Verify(r => r.GetAll(), Times.Once);
    }

    [Theory]
    [InlineData("Anna")]
    [InlineData("ann")] // substring match behaviour
    public void SearchContacts_ReturnsMatchingContacts(string search)
    {
        // Arrange
        var mockRepo = new Mock<IContactRepository>();

        mockRepo
            .Setup(r => r.SearchByName(search))
            .Returns(new List<Contact>
            {
                new Contact
                {
                    Id = 1,
                    Name = "Anna",
                    Email = "test@gmail.com",
                    Tags = new List<string> { "friend", "football" }
                }
            });

        var service = new ContactService(
            mockRepo.Object,
            Mock.Of<ILogger<ContactService>>()
        );

        // Act
        var result = service.SearchContacts(search).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Anna", result[0].Name);
        Assert.Equal("test@gmail.com", result[0].Email);

        mockRepo.Verify(r => r.SearchByName(search), Times.Once);
    }

    [Fact]
    public void FilterByTag_ReturnsOnlyMatchingContacts()
    {
        // Arrange
        var mockRepo = new Mock<IContactRepository>();

        mockRepo
            .Setup(r => r.FilterByTag("friend"))
            .Returns(new List<Contact>
            {
                new Contact
                {
                    Id = 1,
                    Name = "Anna",
                    Email = "test@gmail.com",
                    Tags = new List<string> { "friend", "football" }
                },
                new Contact
                {
                    Id = 3,
                    Name = "Charlie",
                    Email = "charlie@gmail.com",
                    Tags = new List<string> { "friend" }
                }
            });

        var service = new ContactService(
            mockRepo.Object,
            Mock.Of<ILogger<ContactService>>()
        );

        // Act
        var result = service.FilterByTag("friend").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, c =>
            Assert.Contains("friend", c.Tags, StringComparer.OrdinalIgnoreCase));

        // alphabetical sort by name is enforced in repository, so we expect Anna then Charlie
        Assert.Equal("Anna", result[0].Name);
        Assert.Equal("Charlie", result[1].Name);

        mockRepo.Verify(r => r.FilterByTag("friend"), Times.Once);
    }
}
