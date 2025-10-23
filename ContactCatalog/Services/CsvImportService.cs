using ContactCatalog.Models;
using ContactCatalog.Repositories;
using Microsoft.Extensions.Logging;

namespace ContactCatalog.Services;

public class CsvImportService
{
    private readonly IContactRepository _repository;
    private readonly ILogger<CsvImportService> _logger;

    public CsvImportService(IContactRepository repository, ILogger<CsvImportService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public CsvImportResult ImportFromFile(string filePath)
    {
        int successCount = 0;
        var failures = new List<CsvImportFailure>();
        var startTime = DateTime.Now;

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        _logger.LogInformation("Starting CSV import from: {FilePath}", filePath);

        var lines = File.ReadAllLines(filePath);
        
        for (int i = 0; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (lineNumber == 1 && line.ToLower().Contains("name"))
            {
                continue;
            }

            try
            {
                var parts = line.Split(',');
                
                if (parts.Length < 2)
                {
                    throw new Exception("Invalid CSV format");
                }

                var name = parts[0].Trim();
                var email = parts[1].Trim();
                var tags = new List<string>();

                for (int j = 2; j < parts.Length; j++)
                {
                    var tag = parts[j].Trim();
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        tags.Add(tag);
                    }
                }

                var contact = new Contact
                {
                    Name = name,
                    Email = email,
                    Tags = tags
                };

                _repository.Add(contact);
                successCount++;
                _logger.LogInformation("Imported contact: {Name} ({Email})", contact.Name, contact.Email);
            }
            catch (Exception ex)
            {
                failures.Add(new CsvImportFailure
                {
                    LineNumber = lineNumber,
                    Line = line,
                    Reason = ex.Message
                });
                _logger.LogWarning("Failed to import line {LineNumber}: {Reason}", lineNumber, ex.Message);
            }
        }

        var duration = (DateTime.Now - startTime).TotalMilliseconds;
        
        _logger.LogInformation("CSV import completed. Success: {Success}, Failed: {Failed}, Duration: {Duration}ms",
            successCount, failures.Count, duration);

        return new CsvImportResult
        {
            SuccessCount = successCount,
            Failures = failures,
            DurationMs = (long)duration
        };
    }
}

public class CsvImportResult
{
    public int SuccessCount { get; set; }
    public List<CsvImportFailure> Failures { get; set; } = new();
    public long DurationMs { get; set; }
}

public class CsvImportFailure
{
    public int LineNumber { get; set; }
    public string Line { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

