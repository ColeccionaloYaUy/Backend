using ColeccionaloYa.DataAccess.Interfaces;
using System.Data;
using System.Data.Common;

namespace ColeccionaloYa.DataAccess;

public class CDataReader : ICDataReader {
	private readonly HashSet<string> _ColumnsReader;
	private readonly Dictionary<string, int> _OrdinalsCache;
	private readonly DbDataReader _Reader;

	public CDataReader(DbDataReader reader) {
		_Reader = reader;
		_OrdinalsCache = [];
		_ColumnsReader = [];
		LoadColumns();
	}

	public async Task<bool> ReadAsync(CancellationToken cancellationToken = default) {
		return await _Reader.ReadAsync(cancellationToken);
	}

	public async Task CloseAsync() {
		await _Reader.CloseAsync();
	}

	public async ValueTask DisposeAsync() {
		await _Reader.DisposeAsync();
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Returns the value, of type T, from the DbDataReader, accounting for both generic and non-generic types.
	/// </summary>
	/// <typeparam name="T">T, type applied</typeparam>
	/// <param name="alias">The column of data to retrieve a value from</param>
	/// <returns>T, type applied; default value of type if database value is null</returns>
	public T GetValue<T>(string alias) {
		if (!_ColumnsReader.Contains(alias)) {
			return default!;
		}

		if (!_OrdinalsCache.TryGetValue(alias, out var ordinal)) {
			ordinal = _Reader.GetOrdinal(alias);
			_OrdinalsCache.Add(alias, ordinal);
		}

		if (ordinal == -1 || _Reader.IsDBNull(ordinal))
			return default!;

		var value = _Reader.GetValue(ordinal);

		// Si ya es del tipo correcto → evitar conversiones
		if (value is T variable)
			return variable;

		var valueType = typeof(T);

		// DateOnly no implementa IConvertible, se convierte desde DateTime
		if (valueType == typeof(DateOnly)) {
			var dt = Convert.ToDateTime(value);
			return (T)(object)DateOnly.FromDateTime(dt);
		}

		// Si es nullable
		if (IsNullableType(valueType)) {
			var underlying = Nullable.GetUnderlyingType(valueType)!;
			if (underlying == typeof(DateOnly)) {
				var dt = Convert.ToDateTime(value);
				return (T)(object)DateOnly.FromDateTime(dt);
			}
			return (T)Convert.ChangeType(value, underlying);
		}

		// Conversión normal
		return (T)Convert.ChangeType(value, valueType);
	}

	private static bool IsNullableType(Type theValueType) {
		return theValueType.IsGenericType && theValueType.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
	}

	private void LoadColumns() {
		var columns = _Reader.GetSchemaTable()?.Rows.OfType<DataRow>().Select(x => x["ColumnName"].ToString());
		if (columns is null)
			return;

		foreach (var column in columns) {
			if (!string.IsNullOrEmpty(column))
				_ColumnsReader.Add(column);
		}
	}
}
