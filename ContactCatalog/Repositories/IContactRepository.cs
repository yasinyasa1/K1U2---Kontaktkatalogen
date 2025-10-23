using ContactCatalog.Models;

namespace ContactCatalog.Repositories;

public interface IContactRepository
{
    IEnumerable<Contact> GetAll();
    Contact? GetById(int id);
    void Add(Contact contact);
    IEnumerable<Contact> SearchByName(string query);
    IEnumerable<Contact> FilterByTag(string tag);
    HashSet<string> GetExistingEmails();
}

