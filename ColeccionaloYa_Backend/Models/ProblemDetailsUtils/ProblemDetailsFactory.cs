namespace API.API_Clean_Architecture.Models.ProblemDetailsUtils;

public static class ProblemDetailsFactory {
	private const string BaseTypeUri = "https://httpstatuses.com/";

	public static ApiProblemDetails BadRequest(string? detail = null, string? instance = null) {
		return new ApiProblemDetails(
			"Bad Request",
			400,
			detail ?? "The request could not be understood by the server due to malformed syntax.",
			$"{BaseTypeUri}400",
			instance
		);
	}

	public static ApiProblemDetails Unauthorized(string? detail = null, string? instance = null) {
		return new ApiProblemDetails(
			"Unauthorized",
			401,
			detail ?? "The request requires user authentication.",
			$"{BaseTypeUri}401",
			instance
		);
	}

	public static ApiProblemDetails Forbidden(string? detail = null, string? instance = null) {
		return new ApiProblemDetails(
			"Forbidden",
			403,
			detail ?? "The server understood the request, but is refusing to fulfill it.",
			$"{BaseTypeUri}403",
			instance
		);
	}

	public static ApiProblemDetails NotFound(string? detail = null, string? instance = null) {
		return new ApiProblemDetails(
			"Not Found",
			404,
			detail ?? "The requested resource could not be found.",
			$"{BaseTypeUri}404",
			instance
		);
	}

	public static ApiProblemDetails Conflict(string? detail = null, string? instance = null) {
		return new ApiProblemDetails(
			"Conflict",
			409,
			detail ?? "The request could not be completed due to a conflict with the current state of the resource.",
			$"{BaseTypeUri}409",
			instance
		);
	}

	public static ApiProblemDetails UnprocessableEntity(string? detail = null, string? instance = null) {
		return new ApiProblemDetails(
			"Unprocessable Entity",
			422,
			detail ?? "The request was well-formed but was unable to be followed due to semantic errors.",
			$"{BaseTypeUri}422",
			instance
		);
	}

	public static ApiProblemDetails InternalServerError(string? detail = null, string? instance = null) {
		return new ApiProblemDetails(
			"Internal Server Error",
			500,
			detail ?? "The server encountered an unexpected condition that prevented it from fulfilling the request.",
			$"{BaseTypeUri}500",
			instance
		);
	}

	public static ApiProblemDetails ValidationProblem(IDictionary<string, string[]> errors, string? instance = null) {
		var problem = UnprocessableEntity("One or more validation errors occurred.", instance);
		problem.Errors = errors;
		return problem;
	}

	public static ApiProblemDetails Custom(string title, int status, string? detail = null, string? type = null,
		string? instance = null) {
		return new ApiProblemDetails(title, status, detail, type, instance);
	}
}