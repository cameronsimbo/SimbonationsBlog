using Learn.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Learn.WebAPI.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

    public ApiExceptionFilterAttribute()
    {
        _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
            { typeof(DailyLimitExceededException), HandleDailyLimitExceededException }
        };
    }

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);
        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        Type type = context.Exception.GetType();

        if (_exceptionHandlers.TryGetValue(type, out Action<ExceptionContext>? handler))
        {
            handler.Invoke(context);
            return;
        }

        HandleUnknownException(context);
    }

    private static void HandleValidationException(ExceptionContext context)
    {
        ValidationException exception = (ValidationException)context.Exception;

        ValidationProblemDetails details = new(exception.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleNotFoundException(ExceptionContext context)
    {
        NotFoundException exception = (NotFoundException)context.Exception;

        ProblemDetails details = new()
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            Title = "The specified resource was not found.",
            Detail = exception.Message
        };

        context.Result = new NotFoundObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleForbiddenAccessException(ExceptionContext context)
    {
        ProblemDetails details = new()
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

        context.ExceptionHandled = true;
    }

    private static void HandleDailyLimitExceededException(ExceptionContext context)
    {
        DailyLimitExceededException exception = (DailyLimitExceededException)context.Exception;

        ProblemDetails details = new()
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Daily submission limit exceeded.",
            Detail = exception.Message,
            Type = "https://tools.ietf.org/html/rfc6585#section-4"
        };

        details.Extensions["resetTime"] = exception.ResetTime;

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status429TooManyRequests
        };

        context.ExceptionHandled = true;
    }

    private static void HandleUnknownException(ExceptionContext context)
    {
        ProblemDetails details = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;
    }
}
