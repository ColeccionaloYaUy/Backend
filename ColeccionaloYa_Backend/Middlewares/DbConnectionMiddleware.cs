using API.DataAccess.Interfaces;

namespace API.API_Clean_Architecture.Middlewares;

public class DbConnectionMiddleware {
	private readonly RequestDelegate _Next;

	public DbConnectionMiddleware(RequestDelegate next) {
		_Next = next;
	}

	public async Task InvokeAsync(HttpContext context, ICConnection connection) {
		await connection.Connect();

        try {
			await _Next(context);
		} finally {
			await connection.Disconnect();

        }
	}
}
