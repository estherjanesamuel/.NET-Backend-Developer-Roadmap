namespace Customers.Api.Contracts.Responses;
    public class CustomerResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; }
    public string FullName { get; init; }
    public string Email { get; init; }
    public DateTime DateOfBirth { get; init; }
}