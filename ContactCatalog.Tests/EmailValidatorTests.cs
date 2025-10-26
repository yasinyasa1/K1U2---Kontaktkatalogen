using ContactCatalog.Models;
using ContactCatalog.Validators;
using Xunit;

namespace ContactCatalog.Tests;

public class EmailValidatorTests
{
    [Theory]
    [InlineData("codaygmail.com")]        // missing '@'
    [InlineData("")]                      // empty
    [InlineData("coday awesome.com")]     // space inside
    public void Validate_InvalidEmail_ThrowsInvalidContactException(string email)
    {
        // Arrange
        var validator = new ContactValidator();
        var existingEmails = new HashSet<string>(); // nothing added yet

        var contact = new Contact
        {
            Name = "Yasin Ahmadi",
            Email = email,
            Tags = new List<string>()
        };

        // Act + Assert
        Assert.Throws<InvalidContactException>(() =>
            validator.Validate(contact, existingEmails));
    }

    [Theory]
    [InlineData("yasin@example.com")]
    [InlineData("yasin.ahmadi@test.com")]
    [InlineData("ya123@gmail.com")]
    public void Validate_ValidEmail_DoesNotThrow(string email)
    {
        // Arrange
        var validator = new ContactValidator();
        var existingEmails = new HashSet<string>();

        var contact = new Contact
        {
            Name = "Yasin Ahmadi",
            Email = email,
            Tags = new List<string>()
        };

        // Act
        var ex = Record.Exception(() =>
            validator.Validate(contact, existingEmails));

        // Assert
        Assert.Null(ex);
    }
}
