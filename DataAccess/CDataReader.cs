using API.DataAccess.Interfaces;
using System.Data;
using System.Data.Common;

namespace API.DataAccess;

public class CDataReader : ICDataReader {
    private readonly HashSet<string> _columnsReader;
    private readonly Dictionary<string, int> _ordinalsCache;
    private readonly DbDataReader _Reader;

    public CDataReader(DbDataReader Reader) {
        _Reader = Reader;
        _ordinalsCache = [];
        _columnsReader = [];
        LoadColumns();
    }

    public async Task<bool> ReadAsync() {
        var reader = await _Reader.ReadAsync();

        return reader;
    }

    public async Task CloseAsync() {
        await _Reader.CloseAsync();
    }

    /// <summary>
    ///     Returns the value, of type T, from the DbDataReader, accounting for both generic and non-generic types.
    /// </summary>
    /// <typeparam name="T">T, type applied</typeparam>
    /// <param name="dr">The DbDataReader object that queried the database</param>
    /// <param name="alias">The column of data to retrieve a value from</param>
    /// <returns>T, type applied; default value of type if database value is null</returns>
    public T GetValue<T>(string alias) {
        if (!_columnsReader.Contains(alias)) {
            return default!;
        }

        var ordinal = -1;

        // Cacheamos el ordinal para este alias
        if (!_ordinalsCache.ContainsKey(alias)) {
            ordinal = _Reader.GetOrdinal(alias);
            _ordinalsCache.Add(alias, ordinal);
        } else {
            ordinal = _ordinalsCache[alias];
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

    public bool Read() {
        var reader = _Reader.Read();
        return reader;
    }

    private bool IsNullableType(Type theValueType) {
        return theValueType.IsGenericType && theValueType.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
    }

    private void LoadColumns() {
        var columns = _Reader.GetSchemaTable()?.Rows.OfType<DataRow>().Select(x => x["ColumnName"].ToString());

        foreach (var column in columns) {
            _columnsReader.Add(column);
        }
    }
}