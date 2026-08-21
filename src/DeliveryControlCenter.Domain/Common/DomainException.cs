namespace DeliveryControlCenter.Domain.Common;

/// <summary>Raised when an operation would violate a domain invariant.</summary>
public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DomainException()
    {
    }

    public static void ThrowIfNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{name} must be provided.");
        }
    }
}
