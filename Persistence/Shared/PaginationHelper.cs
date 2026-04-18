using API.DataAccess.Interfaces;

namespace API.Persistence.Shared;

public static class PaginationHelper {
	/// <summary>
	/// Executes a single paginated query using COUNT(*) OVER() AS total_count window function.
	/// Falls back to page 1 if the requested page yields no results (page out of range).
	/// </summary>
	/// <param name="connection">Active connection (Connect() must have been called beforehand).</param>
	/// <param name="sql">Full SQL including SELECT (with COUNT(*) OVER() AS total_count), FROM, optional WHERE, and ORDER BY — without LIMIT/OFFSET.</param>
	/// <param name="applyParams">Optional action to bind filter parameters onto the command.</param>
	/// <param name="map">Row mapper: receives a new T instance and the current reader row.</param>
	/// <param name="page">Requested page (1-based).</param>
	/// <param name="pageSize">Page size.</param>
	public static async Task<PagedData<T>> FetchPagedAsync<T>(
		ICConnection connection,
		string sql,
		Action<ICCommand>? applyParams,
		Action<T, ICDataReader> map,
		int page,
		int pageSize
	) where T : new() {
		var (items, totalCount) = await RunPage<T>(connection, sql, applyParams, map, page, pageSize);

		if (items.Count == 0 && page > 1) {
			(items, totalCount) = await RunPage<T>(connection, sql, applyParams, map, 1, pageSize);
			return new PagedData<T>(items, totalCount, 1);
		}

		return new PagedData<T>(items, totalCount, page);
	}

	private static async Task<(List<T> items, int totalCount)> RunPage<T>(
		ICConnection connection,
		string sql,
		Action<ICCommand>? applyParams,
		Action<T, ICDataReader> map,
		int page,
		int pageSize
	) where T : new() {
		var cmd = connection.CreateCommand();
		cmd.CommandText = sql + " LIMIT @pageSize OFFSET @offset";
		applyParams?.Invoke(cmd);
		cmd.AddParameter("pageSize", pageSize);
		cmd.AddParameter("offset", (page - 1) * pageSize);

		var items = new List<T>();
		var totalCount = 0;

		await cmd.ExecuteCommandQuery(rs => {
			totalCount = rs.GetValue<int>("total_count");
			var item = new T();
			map(item, rs);
			items.Add(item);
		});

		return (items, totalCount);
	}
}
