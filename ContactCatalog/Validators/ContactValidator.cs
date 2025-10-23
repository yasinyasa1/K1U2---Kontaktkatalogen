using System.Text.RegularExpressions;
using ContactCatalog.Models;

namespace ContactCatalog.Validators;

public class ContactValidator
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public void Validate(Contact contact, HashSet<string> existingEmails)
    {
        if (contact == null)
        {
            throw new InvalidContactException("Contact cannot be null");
        }

        if (string.IsNullOrWhiteSpace(contact.Name))
        {
            throw new InvalidContactException("Name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(contact.Email))
        {
            throw new InvalidContactException("Email cannot be empty");
        }

        if (!EmailRegex.IsMatch(contact.Email))
        {
            throw new InvalidContactException("Invalid email format");
        }

        if (existingEmails.Contains(contact.Email))
        {
            throw new InvalidContactException($"Email {contact.Email} already exists");
        }
    }
}

