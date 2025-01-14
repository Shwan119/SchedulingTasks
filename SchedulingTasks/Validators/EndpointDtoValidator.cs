using FluentValidation;
using SchedulingTasks.Dto;

namespace SchedulingTasks.Validators
{
    public class EndpointDtoValidator : AbstractValidator<EndpointDto>
    {
        public EndpointDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

            RuleFor(x => x.BaseUrl)
                .NotEmpty().WithMessage("BaseUrl is required.")
                .Must(BeAValidUrl).WithMessage("Invalid BaseUrl format.")
                .MaximumLength(500).WithMessage("BaseUrl cannot exceed 500 characters.");

            RuleFor(x => x.Path)
                .MaximumLength(500).WithMessage("Path cannot exceed 500 characters.");

            RuleFor(x => x.HttpMethod)
                .NotEmpty().WithMessage("HttpMethod is required.")
                .Must(httpMethod => new[] { "GET", "POST", "PUT", "DELETE", "PATCH" }
                    .Contains(httpMethod.ToUpper()))
                .WithMessage("Invalid HTTP method.");

            RuleFor(x => x.TimeoutSeconds)
                .GreaterThan(0).WithMessage("TimeoutSeconds must be greater than 0.");
        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var _);
        }
    }
}
