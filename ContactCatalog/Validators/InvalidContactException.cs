namespace ContactCatalog.Validators;

public class InvalidContactException : Exception
{
    public InvalidContactException(string message) : base(message)
    {
    }

    public InvalidContactException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

