namespace LibraryClub.Api.Exceptions;

public sealed class ConflictException(string message) : DomainException(message);