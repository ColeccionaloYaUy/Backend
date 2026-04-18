using API.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.API_Clean_Architecture.Filters;

public class ModelStateValidationFilter : ActionFilterAttribute {
	public override void OnActionExecuting(ActionExecutingContext context) {
		if (!context.ModelState.IsValid) {
			var errors = context.ModelState
				.Where(x => x.Value?.Errors.Count > 0)
				.ToDictionary(
					kvp => kvp.Key,
					kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
				);

			// Lanzar excepción que será manejada por el middleware
			throw new ModelValidationException(errors);
		}

		base.OnActionExecuting(context);
	}
}