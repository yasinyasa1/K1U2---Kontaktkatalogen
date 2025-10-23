using ContactCatalog.Repositories;
using ContactCatalog.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ContactCatalog.Tests;

public class CsvImportServiceTests
{
    private readonly Mock<IContactRepository> _mockRepo;
    private readonly Mock<ILogger<CsvImportService>> _mockLogger;
    private readonly CsvImportService _service;
    private readonly string _testDirectory;

    public CsvImportServiceTests()
    {
        _mockRepo = new Mock<IContactRepository>();
        _mockLogger = new Mock<ILogger<CsvImportService>>();
        _service = new CsvImportService(_mockRepo.Object, _mockLogger.Object);
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void ImportFromFile_ValidCsv_ImportsAllContacts()
    {
        var csvFile = Path.Combine(_testDirectory, "test.csv");
        File.WriteAllText(csvFile, @"Name,Email,Tags
Yasin Ahmadi,yasin@test.com,work
Jane Smith,jane@test.com,friend,family");

        var result = _service.ImportFromFile(csvFile);

        Assert.Equal(2, result.SuccessCount);
        Assert.Empty(result.Failures);
        _mockRepo.Verify(r => r.Add(It.IsAny<ContactCatalog.Models.Contact>()), Times.Exactly(2));
        
        CleanupTestFile(csvFile);
    }

    [Fact]
    public void ImportFromFile_FileNotFound_ThrowsException()
    {
        var nonExistentFile = Path.Combine(_testDirectory, "notfound.csv");

        Assert.Throws<FileNotFoundException>(() => _service.ImportFromFile(nonExistentFile));
    }

    [Fact]
    public void ImportFromFile_SkipsHeaderRow()
    {
        var csvFile = Path.Combine(_testDirectory, "header.csv");
        File.WriteAllText(csvFile, @"Name,Email,Tags
Test User,test@test.com,work");

        var result = _service.ImportFromFile(csvFile);

        Assert.Equal(1, result.SuccessCount);
        _mockRepo.Verify(r => r.Add(It.IsAny<ContactCatalog.Models.Contact>()), Times.Once);
        
        CleanupTestFile(csvFile);
    }

    [Fact]
    public void ImportFromFile_SkipsEmptyLines()
    {
        var csvFile = Path.Combine(_testDirectory, "empty.csv");
        File.WriteAllText(csvFile, @"Name,Email,Tags
Yasin Ahmadi,yasin@test.com,work

Jane Smith,jane@test.com,friend");

        var result = _service.ImportFromFile(csvFile);

        Assert.Equal(2, result.SuccessCount);
        _mockRepo.Verify(r => r.Add(It.IsAny<ContactCatalog.Models.Contact>()), Times.Exactly(2));
        
        CleanupTestFile(csvFile);
    }

    [Fact]
    public void ImportFromFile_InvalidFormat_RecordsFailure()
    {
        var csvFile = Path.Combine(_testDirectory, "invalid.csv");
        File.WriteAllText(csvFile, @"Name,Email,Tags
InvalidLine
Yasin Ahmadi,yasin@test.com,work");

        var result = _service.ImportFromFile(csvFile);

        Assert.Equal(1, result.SuccessCount);
        Assert.Single(result.Failures);
        Assert.Equal(2, result.Failures[0].LineNumber);
        Assert.Contains("Invalid CSV format", result.Failures[0].Reason);
        
        CleanupTestFile(csvFile);
    }

    [Fact]
    public void ImportFromFile_ValidationError_RecordsFailure()
    {
        var csvFile = Path.Combine(_testDirectory, "validation.csv");
        File.WriteAllText(csvFile, @"Name,Email,Tags
,missing.name@test.com,work
Valid User,valid@test.com,friend");

        _mockRepo.Setup(r => r.Add(It.Is<ContactCatalog.Models.Contact>(c => string.IsNullOrWhiteSpace(c.Name))))
            .Throws(new ContactCatalog.Validators.InvalidContactException("Name cannot be empty"));

        var result = _service.ImportFromFile(csvFile);

        Assert.Equal(1, result.SuccessCount);
        Assert.Single(result.Failures);
        Assert.Contains("Name cannot be empty", result.Failures[0].Reason);
        
        CleanupTestFile(csvFile);
    }

    [Fact]
    public void ImportFromFile_ParsesTagsCorrectly()
    {
        var csvFile = Path.Combine(_testDirectory, "tags.csv");
        File.WriteAllText(csvFile, @"Name,Email,Tags
Yasin Ahmadi,yasin@test.com,work,colleague,friend");

        ContactCatalog.Models.Contact? capturedContact = null;
        _mockRepo.Setup(r => r.Add(It.IsAny<ContactCatalog.Models.Contact>()))
            .Callback<ContactCatalog.Models.Contact>(c => capturedContact = c);

        var result = _service.ImportFromFile(csvFile);

        Assert.NotNull(capturedContact);
        Assert.Equal(3, capturedContact.Tags.Count);
        Assert.Contains("work", capturedContact.Tags);
        Assert.Contains("colleague", capturedContact.Tags);
        Assert.Contains("friend", capturedContact.Tags);
        
        CleanupTestFile(csvFile);
    }

    [Fact]
    public void ImportFromFile_RecordsDurationMs()
    {
        var csvFile = Path.Combine(_testDirectory, "duration.csv");
        File.WriteAllText(csvFile, @"Name,Email,Tags
Test User,test@test.com,work");

        var result = _service.ImportFromFile(csvFile);

        Assert.True(result.DurationMs >= 0);
        
        CleanupTestFile(csvFile);
    }

    [Fact]
    public void ImportFromFile_HandlesContactsWithoutTags()
    {
        var csvFile = Path.Combine(_testDirectory, "notags.csv");
        File.WriteAllText(csvFile, @"Name,Email,Tags
Yasin Ahmadi,yasin@test.com");

        ContactCatalog.Models.Contact? capturedContact = null;
        _mockRepo.Setup(r => r.Add(It.IsAny<ContactCatalog.Models.Contact>()))
            .Callback<ContactCatalog.Models.Contact>(c => capturedContact = c);

        var result = _service.ImportFromFile(csvFile);

        Assert.NotNull(capturedContact);
        Assert.Empty(capturedContact.Tags);
        Assert.Equal("Yasin Ahmadi", capturedContact.Name);
        Assert.Equal("yasin@test.com", capturedContact.Email);
        
        CleanupTestFile(csvFile);
    }

    private void CleanupTestFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}

