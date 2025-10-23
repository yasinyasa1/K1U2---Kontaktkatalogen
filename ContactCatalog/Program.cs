using ContactCatalog.Repositories;
using ContactCatalog.Services;
using ContactCatalog.UI;
using Microsoft.Extensions.Logging;

namespace ContactCatalog;

class Program
{
    static void Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var repository = new ContactRepository();
        var contactLogger = loggerFactory.CreateLogger<ContactService>();
        var csvLogger = loggerFactory.CreateLogger<CsvImportService>();
        
        var contactService = new ContactService(repository, contactLogger);
        var csvService = new CsvImportService(repository, csvLogger);

        if (args.Length > 0)
        {
            var commandLineHandler = new CommandLineHandler(csvService, contactService);
            commandLineHandler.Handle(args);
        }
        else
        {
            var menu = new ConsoleMenu(contactService);
            menu.Run();
        }
    }
}
