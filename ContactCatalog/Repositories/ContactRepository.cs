using ContactCatalog.Models;
using ContactCatalog.Validators;

namespace ContactCatalog.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly Dictionary<int, Contact> _contacts = new();
    private readonly HashSet<string> _emails = new();
    private int _nextId = 1;
    private readonly ContactValidator _validator = new();

    public IEnumerable<Contact> GetAll()
    {
        return _contacts.Values;
    }

    public Contact? GetById(int id)
    {
        return _contacts.TryGetValue(id, out var contact) ? contact : null;
    }

    public void Add(Contact contact)
    {
        _validator.Validate(contact, _emails);
        contact.Id = _nextId++;
        _contacts[contact.Id] = contact;
        _emails.Add(contact.Email);
    }

    public IEnumerable<Contact> SearchByName(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<Contact>();
        }

        return _contacts.Values
            .Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Name);
    }

    public IEnumerable<Contact> FilterByTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return Enumerable.Empty<Contact>();
        }

        return _contacts.Values
            .Where(c => c.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            .OrderBy(c => c.Name);
    }

    public HashSet<string> GetExistingEmails()
    {
        return _emails;
    }
}

