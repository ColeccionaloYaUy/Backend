using System.Net;
using API.API_Clean_Architecture.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.API_Clean_Architecture.Filters;

public class ResponseFilter : IAsyncActionFilter {
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
		var executedContext = await next();

		if (executedContext.Exception != null) {
			return;
		}

		if (executedContext.Result is ObjectResult objectResult) {
			var returnValue = objectResult.Value;

			var commonResponse = new Response(
				HttpStatusCode.OK,
				"Success",
				returnValue ?? "OK"
			);

			executedContext.Result = new ObjectResult(commonResponse) {
				StatusCode = (int)HttpStatusCode.OK,
			};
		}
	}
}