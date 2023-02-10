namespace Customers.Api.Contracts.Responses;
public class ValidationFailureResponse
{
    public List<string> Errors { get; set; } = new();
}