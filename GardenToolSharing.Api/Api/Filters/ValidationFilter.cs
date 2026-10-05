using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GardenToolSharing.Api.Api.Filters;

/// <summary>Validates the request DTO of type T with DataAnnotations and returns a 400 ProblemDetails on failure.</summary>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["body"] = ["A request body is required."]
            });
        }

        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(argument, new ValidationContext(argument), results, validateAllProperties: true))
            return await next(context);

        // Keys are camelCased so they match the JSON field names the frontend sends
        var errors = results
            .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : new[] { string.Empty })
                .Select(member => (
                    Member: JsonNamingPolicy.CamelCase.ConvertName(member),
                    Message: r.ErrorMessage ?? "Invalid value.")))
            .GroupBy(x => x.Member, x => x.Message)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return Results.ValidationProblem(errors);
    }
}
