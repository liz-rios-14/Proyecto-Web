namespace SalesPoint.Domain.Exceptions;

public sealed class InvalidSaleStateException : DomainException
{
    public InvalidSaleStateException(string message)
        : base(message)
    {
    }
}
