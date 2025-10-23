using ContactCatalog.Models;
using ContactCatalog.Validators;
using Xunit;

namespace ContactCatalog.Tests;

public class ContactValidatorTests
{
    private readonly ContactValidator _validator;

    public ContactValidatorTests()
    {
        _validator = new ContactValidator();
    }

    [Fact]
    public void Validate_ValidContact_DoesNotThrow()
    {
        // Arrange
        var contact = new Contact
        {
            Name = "Yasin Ahmadi",
            Email = "yasin@example.com",
            Tags = new List<string> { "work" }
        };
        var existingEmails = new HashSet<string>();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => _validator.Validate(contact, existingEmails));
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullContact_ThrowsException()
    {
        // Arrange
        Contact? contact = null;
        var existingEmails = new HashSet<string>();

        // Act & Assert
        var ex = Assert.Throws<InvalidContactException>(() => 
            _validator.Validate(contact!, existingEmails));
        Assert.Contains("cannot be null", ex.Message);
    }

    [Fact]
    public void Validate_EmptyName_ThrowsException()
    {
        // Arrange
        var contact = new Contact
        {
            Name = "",
            Email = "yasin@example.com",
            Tags = new List<string>()
        };
        var existingEmails = new HashSet<string>();

        // Act & Assert
        var ex = Assert.Throws<InvalidContactException>(() => 
            _validator.Validate(contact, existingEmails));
        Assert.Contains("Name cannot be empty", ex.Message);
    }

    [Fact]
    public void Validate_WhitespaceName_ThrowsException()
    {
        // Arrange
        var contact = new Contact
        {
            Name = "   ",
            Email = "yasin@example.com",
            Tags = new List<string>()
        };
        var existingEmails = new HashSet<string>();

        // Act & Assert
        var ex = Assert.Throws<InvalidContactException>(() => 
            _validator.Validate(contact, existingEmails));
        Assert.Contains("Name cannot be empty", ex.Message);
    }

    [Fact]
    public void Validate_EmptyEmail_ThrowsException()
    {
        // Arrange
        var contact = new Contact
        {
            Name = "Yasin Ahmadi",
            Email = "",
            Tags = new List<string>()
        };
        var existingEmails = new HashSet<string>();

        // Act & Assert
        var ex = Assert.Throws<InvalidContactException>(() => 
            _validator.Validate(contact, existingEmails));
        Assert.Contains("Email cannot be empty", ex.Message);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    [InlineData("double@@domain.com")]
    public void Validate_InvalidEmailFormat_ThrowsException(string invalidEmail)
    {
        // Arrange
        var contact = new Contact
        {
            Name = "Yasin Ahmadi",
            Email = invalidEmail,
            Tags = new List<string>()
        };
        var existingEmails = new HashSet<string>();

        // Act & Assert
        var ex = Assert.Throws<InvalidContactException>(() => 
            _validator.Validate(contact, existingEmails));
        Assert.Contains("Invalid email format", ex.Message);
    }

    [Theory]
    [InlineData("yasin@example.com")]
    [InlineData("jane.doe@company.org")]
    [InlineData("user123@test-domain.co.uk")]
    [InlineData("admin@sub.domain.net")]
    public void Validate_ValidEmailFormat_DoesNotThrow(string validEmail)
    {
        // Arrange
        var contact = new Contact
        {
            Name = "Yasin Ahmadi",
            Email = validEmail,
            Tags = new List<string>()
        };
        var existingEmails = new HashSet<string>();

        // Act & Assert
        var exception = Record.Exception(() => _validator.Validate(contact, existingEmails));
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var contact = new Contact
        {
            Name = "Yasin Ahmadi",
            Email = "duplicate@example.com",
            Tags = new List<string>()
        };
        var existingEmails = new HashSet<string> { "duplicate@example.com" };

        // Act & Assert
        var ex = Assert.Throws<InvalidContactException>(() => 
            _validator.Validate(contact, existingEmails));
        Assert.Contains("already exists", ex.Message);
        Assert.Contains("duplicate@example.com", ex.Message);
    }

    [Fact]
    public void Validate_ContactWithEmptyTags_DoesNotThrow()
    {
        // Arrange
        var contact = new Contact
        {
            Name = "Yasin Ahmadi",
            Email = "yasin@example.com",
            Tags = new List<string>()
        };
        var existingEmails = new HashSet<string>();

        // Act & Assert - Tags are optional, should not throw
        var exception = Record.Exception(() => _validator.Validate(contact, existingEmails));
        Assert.Null(exception);
    }
}

