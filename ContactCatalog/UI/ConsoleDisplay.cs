using ContactCatalog.Models;

namespace ContactCatalog.UI;

public static class ConsoleDisplay
{
    public static void DisplayContacts(IEnumerable<Contact> contacts)
    {
        Console.WriteLine();
        Console.WriteLine($"{"ID",-5} {"Name",-20} {"Email",-30} {"Tags",-30}");
        Console.WriteLine(new string('-', 90));
        
        foreach (var contact in contacts)
        {
            var tagsStr = string.Join(", ", contact.Tags);
            Console.WriteLine($"{contact.Id,-5} {contact.Name,-20} {contact.Email,-30} {tagsStr,-30}");
        }
    }
}

