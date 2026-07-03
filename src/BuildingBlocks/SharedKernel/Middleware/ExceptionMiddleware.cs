using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Exceptions;

namespace SharedKernel.Middleware
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            (HttpStatusCode StatusCode, string Title, string Detail) mapped = MapException(exception);

            ProblemDetails problemDetails = new()
            {
                Type = $"https://httpstatuses.com/{(int)mapped.StatusCode}",
                Title = mapped.Title,
                Status = (int)mapped.StatusCode,
                Detail = mapped.Detail,
                Instance = context.Request.Path
            };

            if (exception is ValidationException validationEx)
            {
                problemDetails.Extensions["errors"] = validationEx.Errors.ToList();
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)mapped.StatusCode;

            await context.Response.WriteAsJsonAsync(problemDetails).ConfigureAwait(false);
        }

        private static (HttpStatusCode StatusCode, string Title, string Detail) MapException(Exception exception)
        {
            return exception switch
            {
                ValidationException => (HttpStatusCode.BadRequest, "Validation Failed", exception.Message),
                NotFoundException => (HttpStatusCode.NotFound, "Resource Not Found", exception.Message),
                ConflictException => (HttpStatusCode.Conflict, "Conflict", exception.Message),
                DomainException => (HttpStatusCode.BadRequest, "Domain Rule Violation", exception.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid Operation", exception.Message),
                _ => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
            };
        }
    }
}
