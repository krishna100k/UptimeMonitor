using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static Application.Exceptions.Exceptions;

namespace API.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var (status, title) = exception switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
                NotFoundException =>(StatusCodes.Status404NotFound, "Resource not found"),
                ConflictException =>(StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
            };

            _logger.LogError(exception, $"{title} while processing Path", context.Request.Path);

            var problemDetails = new ProblemDetails
            {
                Title = title,
                Status = status,
                Detail = exception.Message,
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
            return true;
        }
    }
}
