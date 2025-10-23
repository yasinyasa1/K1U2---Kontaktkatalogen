using ContactCatalog.Models;
using ContactCatalog.Services;
using ContactCatalog.Validators;

namespace ContactCatalog.UI;

public class ConsoleMenu
{
    private readonly ContactService _contactService;

    public ConsoleMenu(ContactService contactService)
    {
        _contactService = contactService;
    }

    public void Run()
    {
        bool running = true;

        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("=== Contact Catalog ===");
            Console.WriteLine("1. Add Contact");
            Console.WriteLine("2. List All Contacts");
            Console.WriteLine("3. Search by Name");
            Console.WriteLine("4. Filter by Tag");
            Console.WriteLine("0. Exit");
            Console.Write("Enter choice: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        AddContact();
                        break;
                    case "2":
                        ListContacts();
                        break;
                    case "3":
                        SearchContacts();
                        break;
                    case "4":
                        FilterByTag();
                        break;
                    case "0":
                        running = false;
                        Console.WriteLine("Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private void AddContact()
    {
        Console.WriteLine();
        Console.WriteLine("--- Add Contact ---");
        
        Console.Write("Name: ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Email: ");
        var email = Console.ReadLine() ?? string.Empty;

        Console.Write("Tags (comma-separated): ");
        var tagsInput = Console.ReadLine() ?? string.Empty;
        var tags = tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(t => t.Trim())
                           .Where(t => !string.IsNullOrWhiteSpace(t))
                           .ToList();

        try
        {
            _contactService.AddContact(name, email, tags);
            Console.WriteLine("✓ Contact added successfully!");
        }
        catch (InvalidContactException ex)
        {
            Console.WriteLine($"✗ Failed to add contact: {ex.Message}");
        }
    }

    private void ListContacts()
    {
        Console.WriteLine();
        Console.WriteLine("--- All Contacts ---");
        
        var contacts = _contactService.ListContacts();
        
        if (!contacts.Any())
        {
            Console.WriteLine("No contacts found.");
            return;
        }

        ConsoleDisplay.DisplayContacts(contacts);
    }

    private void SearchContacts()
    {
        Console.WriteLine();
        Console.WriteLine("--- Search by Name ---");
        
        Console.Write("Enter search query: ");
        var query = Console.ReadLine() ?? string.Empty;

        var results = _contactService.SearchContacts(query);

        if (!results.Any())
        {
            Console.WriteLine($"No contacts found matching '{query}'.");
            return;
        }

        Console.WriteLine($"Found {results.Count()} contact(s):");
        ConsoleDisplay.DisplayContacts(results);
    }

    private void FilterByTag()
    {
        Console.WriteLine();
        Console.WriteLine("--- Filter by Tag ---");
        
        Console.Write("Enter tag: ");
        var tag = Console.ReadLine() ?? string.Empty;

        var results = _contactService.FilterByTag(tag);

        if (!results.Any())
        {
            Console.WriteLine($"No contacts found with tag '{tag}'.");
            return;
        }

        Console.WriteLine($"Found {results.Count()} contact(s) with tag '{tag}':");
        ConsoleDisplay.DisplayContacts(results);
    }
}

