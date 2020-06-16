using FluentValidation;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1.Requests;

namespace Tweetbook.Validators
{
    public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
    {
        public CreateTagRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Matches("^[a-zA-Z0-9 ]*$");
            // re you can also customize with more rule vaidations for more advanced scenarios
            //RuleFor(x => x.Name)
            //    .Must(s => s.Contains("special test"));
        }
    }
}
