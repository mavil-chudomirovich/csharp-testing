using Application.AppExceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;
using System.Reflection;


namespace API.Filters
{
    public class ValidationFilter : IActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var arg in context.ActionArguments)
            {
                var paramType = arg.Value?.GetType();
                if (paramType == null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(paramType);
                var validator = _serviceProvider.GetService(validatorType);

                if (validator != null)
                {
                    var method = validatorType.GetMethod("Validate", new[] { paramType });
                    var result = (FluentValidation.Results.ValidationResult)method!.Invoke(validator, new[] { arg.Value })!;

                    if (!result.IsValid)
                    {
                        var message = result.Errors.First().ErrorMessage;

                        throw new System.ComponentModel.DataAnnotations.ValidationException(message);
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }

}
