using ContactCatalog.Services;

namespace ContactCatalog.UI;

public class CommandLineHandler
{
    private readonly CsvImportService _csvService;
    private readonly ContactService _contactService;

    public CommandLineHandler(CsvImportService csvService, ContactService contactService)
    {
        _csvService = csvService;
        _contactService = contactService;
    }

    public void Handle(string[] args)
    {
        if (args.Length >= 2 && args[0].ToLower() == "-import")
        {
            ImportCsv(args[1]);
        }
        else
        {
            ShowUsage();
        }
    }

    private void ImportCsv(string filePath)
    {
        Console.WriteLine($"Importing contacts from: {filePath}");
        Console.WriteLine();

        try
        {
            var result = _csvService.ImportFromFile(filePath);
            
            Console.WriteLine("=== Import Summary ===");
            Console.WriteLine($"Successfully imported: {result.SuccessCount} contacts");
            Console.WriteLine($"Failed: {result.Failures.Count} rows");
            Console.WriteLine($"Total processing time: {result.DurationMs}ms");

            if (result.Failures.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Failed rows:");
                foreach (var failure in result.Failures)
                {
                    Console.WriteLine($"  Line {failure.LineNumber}: {failure.Reason}");
                    Console.WriteLine($"    Data: {failure.Line}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("All contacts after import:");
            var contacts = _contactService.ListContacts();
            ConsoleDisplay.DisplayContacts(contacts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during import: {ex.Message}");
        }
    }

    private void ShowUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  ContactCatalog.exe              - Start interactive menu");
        Console.WriteLine("  ContactCatalog.exe -import <file.csv>  - Import contacts from CSV");
    }
}

