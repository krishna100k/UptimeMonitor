using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Endpoint.AddEndpoint
{
    public class AddEndpointValidator : AbstractValidator<AddEndpointCommand>
    {
        public AddEndpointValidator() {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Url).NotEmpty().Must(BeValidUrl).WithMessage("Please provide a valid HTTP or HTTPS URL.");
        }

        private static bool BeValidUrl(string url)
        {
            return Uri.TryCreate(
                url,
                UriKind.Absolute,
                out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp
                    || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
