using API.API_Clean_Architecture.Models.ProblemDetailsUtils;
using API.Domain.Exceptions;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;

namespace API.API_Clean_Architecture.Middlewares;

public class ExceptionMiddleware {
	private readonly RequestDelegate _Next;

	public ExceptionMiddleware(RequestDelegate next) {
		_Next = next;
	}

	public async Task InvokeAsync(HttpContext context) {
		try {
			await _Next(context);
		} catch (Exception ex) {
			await HandleExceptionAsync(context, ex);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception) {
		LogException(exception);

		var problem = exception switch {
			DomainException domainEx => CreateDomainProblem(domainEx, context.Request.Path, exception.StackTrace),

			ArgumentNullException argNullEx => ProblemDetailsFactory.BadRequest(
				$"Required parameter '{argNullEx.ParamName}' was not provided", context.Request.Path),
			ArgumentException argEx => ProblemDetailsFactory.BadRequest(argEx.Message, context.Request.Path),
			UnauthorizedAccessException => ProblemDetailsFactory.Unauthorized(instance: context.Request.Path),
			KeyNotFoundException => ProblemDetailsFactory.NotFound(instance: context.Request.Path),
			InvalidOperationException invalidOp => ProblemDetailsFactory.Conflict(invalidOp.Message,
				context.Request.Path),

			ValidationException =>
				ProblemDetailsFactory.UnprocessableEntity(
					"One or more validation errors occurred.",
					context.Request.Path
				),

			_ => ProblemDetailsFactory.InternalServerError(
				"An error occurred while processing your request.",
				context.Request.Path
			),
		};

		problem.TraceId = Activity.Current?.Id ?? context.TraceIdentifier;

		if (exception is DomainException domainException) {
			problem.AddExtension(domainException.ErrorType, domainException.Message);
		}

		context.Response.StatusCode = problem.Status ?? 500;
		context.Response.ContentType = "application/problem+json";

		var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true,
		});

		await context.Response.WriteAsync(json);
	}

	private void LogException(Exception exception) {
		switch (exception) {
			case DomainException domainEx when (int)domainEx.StatusCode >= 500:
				Log.Error(exception, "Domain error occurred: {Message}", exception.Message);
				break;
			case DomainException:
				Log.Warning(exception, "Domain validation failed: {Message}", exception.Message);
				break;
			case ValidationException:
				Log.Warning(exception, "Validation failed: {Message}", exception.Message);
				break;
			default:
				Log.Error(exception, "Unhandled exception occurred: {Message}", exception.Message);
				break;
		}
	}

	private static ApiProblemDetails CreateDomainProblem(DomainException domainException, string instance, string? stacktrace) {
		var problem = new ApiProblemDetails(
			domainException.StatusCode.ToString(),
			(int)domainException.StatusCode,
			domainException.Message,
			domainException.ErrorType,
			instance,
            stacktrace
        );

		return problem;
	}
}