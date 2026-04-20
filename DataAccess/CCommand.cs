using ColeccionaloYa.DataAccess.Interfaces;
using Npgsql;
using System.Data;

namespace ColeccionaloYa.DataAccess;

public class CCommand : ICCommand {
	private readonly NpgsqlCommand _Command;

	public CCommand(CConnection connection) {
		_Command = new NpgsqlCommand {
			Connection = connection.ConnectionDB,
		};
		if (connection.Transaction != null) {
			_Command.Transaction = connection.Transaction;
		}

		_Command.CommandTimeout = 300;
	}

	public string CommandText {
		get => _Command.CommandText;
		set => _Command.CommandText = value;
	}

	public void AddParameter(string name, object? value, DbType? type = null) {
		var param = _Command.CreateParameter();
		param.ParameterName = name;
		param.Value = value ?? DBNull.Value;
		if (type.HasValue)
			param.DbType = type.Value;
		_Command.Parameters.Add(param);
	}

	public async Task<ICDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default) {
		var npgsqlReader = await _Command.ExecuteReaderAsync(cancellationToken);
		try {
			return new CDataReader(npgsqlReader);
		} catch {
			await npgsqlReader.DisposeAsync();
			throw;
		}
	}

	public async Task ExecuteCommandQuery(Action<ICDataReader> func, CancellationToken cancellationToken = default) {
		await using var rs = await ExecuteReaderAsync(cancellationToken);

		while (await rs.ReadAsync(cancellationToken)) {
			func(rs);
		}
	}

	public async Task<bool> ExecuteCommandExists(CancellationToken cancellationToken = default) {
		await using var rs = await ExecuteReaderAsync(cancellationToken);
		return await rs.ReadAsync(cancellationToken);
	}

	public async Task<bool> ExecuteCommandNonQuery(CancellationToken cancellationToken = default) {
		return await _Command.ExecuteNonQueryAsync(cancellationToken) > 0;
	}

	public async Task<T> ExecuteGetValue<T>(string name, CancellationToken cancellationToken = default) {
		await using var rs = await ExecuteReaderAsync(cancellationToken);

		if (await rs.ReadAsync(cancellationToken)) {
			return rs.GetValue<T>(name);
		}

		return default!;
	}

	public async Task<T?> ExecuteSelect<T>(Action<T, ICDataReader> loadData, CancellationToken cancellationToken = default) where T : new() {
		await using var rs = await ExecuteReaderAsync(cancellationToken);

		if (await rs.ReadAsync(cancellationToken)) {
			var result = new T();
			loadData.Invoke(result, rs);
			return result;
		}

		return default;
	}

	public async Task<List<T>> ExecuteSelectList<T>(Action<T, ICDataReader> loadData, CancellationToken cancellationToken = default) where T : new() {
		var result = new List<T>();

		await using var rs = await ExecuteReaderAsync(cancellationToken);

		while (await rs.ReadAsync(cancellationToken)) {
			T obj = new();
			loadData.Invoke(obj, rs);

			result.Add(obj);
		}

		return result;
	}
}
