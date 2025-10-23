using ContactCatalog.Models;
using ContactCatalog.Repositories;
using ContactCatalog.Validators;
using Microsoft.Extensions.Logging;

namespace ContactCatalog.Services;

public class ContactService
{
    private readonly IContactRepository _repository;
    private readonly ILogger<ContactService> _logger;

    public ContactService(IContactRepository repository, ILogger<ContactService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public void AddContact(string name, string email, List<string> tags)
    {
        try
        {
            var contact = new Contact
            {
                Name = name,
                Email = email,
                Tags = tags
            };

            _repository.Add(contact);
            _logger.LogInformation("Contact added: {Name} ({Email})", contact.Name, contact.Email);
        }
        catch (InvalidContactException ex)
        {
            if (ex.Message.Contains("already exists"))
            {
                _logger.LogWarning("Duplicate email attempt: {Email}", email);
            }
            else
            {
                _logger.LogError(ex, "Validation failed for contact: {Email}", email);
            }
            throw;
        }
    }

    public IEnumerable<Contact> ListContacts()
    {
        var contacts = _repository.GetAll();
        _logger.LogInformation("Listed all contacts. Total count: {Count}", contacts.Count());
        return contacts;
    }

    public IEnumerable<Contact> SearchContacts(string query)
    {
        var results = _repository.SearchByName(query);
        _logger.LogInformation("Search performed for: '{Query}', found {Count} contacts", query, results.Count());
        return results;
    }

    public IEnumerable<Contact> FilterByTag(string tag)
    {
        var results = _repository.FilterByTag(tag);
        _logger.LogInformation("Filtered by tag: '{Tag}', found {Count} contacts", tag, results.Count());
        return results;
    }
}

