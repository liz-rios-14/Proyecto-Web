namespace SalesPoint.Domain.Exceptions;

public sealed class DuplicateProductException : DomainException
{
    public DuplicateProductException(string message)
        : base(message)
    {
    }
}
