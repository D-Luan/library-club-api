namespace LibraryClub.Api.Exceptions;

public sealed class DomainValidationException(string message) : DomainException(message);
