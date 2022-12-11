using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using ValueOf;

namespace Customers.Api.Domain.Common;

public class FullName : ValueOf<string, FullName>
{
    private static readonly Regex FullNameRegex = new("^[a-z ,.'-]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    protected override void Validate()
    {
        if (!FullNameRegex.IsMatch(Value))
        {
            var message = $"{Value} is not valid fullname";
            throw new ValidationException(message, new [] 
            {
                new ValidationFailure(nameof(FullName), message)
            });
        }
    }
}