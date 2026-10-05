namespace GardenToolSharing.Api.Common.Exceptions;

/// <summary>Business-rule errors that map to a specific HTTP status. Handled by GlobalExceptionHandler.</summary>
public abstract class AppException(string message, int statusCode, string title) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
}

public class ConflictException(string message) : AppException(message, 409, "Conflict");

public class UnauthorizedException(string message) : AppException(message, 401, "Unauthorized");

public class ForbiddenException(string message) : AppException(message, 403, "Forbidden");

public class NotFoundException(string message) : AppException(message, 404, "Not found");

public class DomainValidationException(IDictionary<string, string[]> errors)
    : AppException("One or more validation errors occurred.", 400, "One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;

    public static DomainValidationException ForField(string field, string message) =>
        new(new Dictionary<string, string[]> { [field] = [message] });
}
