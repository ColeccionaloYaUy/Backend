using ColeccionaloYa.DataAccess.Interfaces;

namespace ColeccionaloYa.API_Clean_Architecture.Middlewares;

public class DbConnectionMiddleware {
	private readonly RequestDelegate _Next;

	public DbConnectionMiddleware(RequestDelegate next) {
		_Next = next;
	}

	public async Task InvokeAsync(HttpContext context, ICConnection connection) {
		await connection.Connect(context.RequestAborted);

        try {
			await _Next(context);
		} finally {
			await connection.Disconnect();

        }
	}
}
