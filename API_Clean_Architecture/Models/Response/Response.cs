using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace API.API_Clean_Architecture.Models.Response;

public class Response : IActionResult {
	private const string TEST = "TEST";
	public HttpStatusCode StatusCode { get; set; }

	public string Message { get; set; }

	public object Data { get; set; }

	public Response(HttpStatusCode statusCode, string message, object data) {
		StatusCode = statusCode;
		Message = message;
		Data = data;
	}

	public async Task ExecuteResultAsync(ActionContext context) {
		context.HttpContext.Response.StatusCode = (int)StatusCode;
		context.HttpContext.Response.ContentType = "application/json";

		var objectResult = new ObjectResult(Data) {
			StatusCode = (int)StatusCode,
		};

		await objectResult.ExecuteResultAsync(context);
	}
}