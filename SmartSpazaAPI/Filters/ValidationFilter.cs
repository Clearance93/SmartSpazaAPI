using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartSpazaAPI.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var services = context.HttpContext.RequestServices;

            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null)
                {
                    continue;
                }

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

                if (services.GetService(validatorType) is not IValidator validator)
                {
                    continue;
                }

                var validationContext = new ValidationContext<object>(argument);
                ValidationResult result = await validator.ValidateAsync(validationContext);

                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(error => new
                    {
                        error.PropertyName,
                        error.ErrorMessage
                    });

                    context.Result = new BadRequestObjectResult(new
                    {
                        Message = "One or more validation errors occurred.",
                        Errors = errors
                    });

                    return;
                }
            }

            await next();
        }
    }
}
