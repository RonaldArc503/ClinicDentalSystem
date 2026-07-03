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
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title, detail) = MapException(exception);

            var problemDetails = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{(int)statusCode}",
                Title = title,
                Status = (int)statusCode,
                Detail = detail,
                Instance = context.Request.Path
            };

            if (exception is ValidationException validationEx)
            {
                problemDetails.Extensions["errors"] = validationEx.Errors.ToList();
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsJsonAsync(problemDetails);
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
