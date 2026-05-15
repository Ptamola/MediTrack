using System.Data;
using MySqlConnector;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories.MySql;

/// <summary>
/// Clase base para repositorios MySQL con operaciones comunes de lectura y guardado.
/// Mantiene transacciones y usa sentencias UPSERT para evitar borrados masivos.
/// </summary>
public abstract class MySqlRepositoryBase<T>
{
    private readonly DatabaseConnectionFactory _connectionFactory;

    protected MySqlRepositoryBase(DatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    protected abstract string SelectSql { get; }
    protected abstract string InsertSql { get; }
    protected abstract T Map(MySqlDataReader reader);
    protected abstract void AddInsertParameters(MySqlCommand command, T item);

    /// <summary>
    /// Obtiene todos los registros de la tabla asociada al repositorio.
    /// </summary>
    public async Task<List<T>> GetAllAsync()
    {
        var items = new List<T>();
        await using var connection = _connectionFactory.CreateDatabaseConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = SelectSql;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(Map(reader));
        }

        return items;
    }

    /// <summary>
    /// Guarda una lista de entidades usando INSERT ... ON DUPLICATE KEY UPDATE.
    /// No elimina registros existentes, por lo que respeta relaciones y datos manuales.
    /// </summary>
    public async Task SaveAllAsync(List<T> items)
    {
        await using var connection = _connectionFactory.CreateDatabaseConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        try
        {
            foreach (var item in items)
            {
                await using var insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;
                insertCommand.CommandText = InsertSql;
                AddInsertParameters(insertCommand, item);
                await insertCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Normaliza valores nulos para que MySqlConnector reciba DBNull.Value.
    /// </summary>
    protected static void AddParameter(MySqlCommand command, string name, object? value)
    {
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }

    /// <summary>
    /// Lee Guid tanto si MySqlConnector devuelve System.Guid como si devuelve texto CHAR(36).
    /// </summary>
    protected static Guid GetGuid(MySqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
        {
            throw new InvalidOperationException($"La columna {columnName} contiene NULL y se esperaba un Guid.");
        }

        var value = reader.GetValue(ordinal);

        if (value is Guid guid)
        {
            return guid;
        }

        if (value is string text)
        {
            return Guid.Parse(text);
        }

        return Guid.Parse(value.ToString()!);
    }

    protected static string GetString(MySqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    protected static DateTime GetDateTime(MySqlDataReader reader, string columnName)
    {
        return reader.GetDateTime(reader.GetOrdinal(columnName));
    }

    protected static DateTime? GetNullableDateTime(MySqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    protected static bool GetBoolean(MySqlDataReader reader, string columnName)
    {
        return reader.GetBoolean(reader.GetOrdinal(columnName));
    }

    protected static int GetInt32(MySqlDataReader reader, string columnName)
    {
        return reader.GetInt32(reader.GetOrdinal(columnName));
    }

    protected static decimal GetDecimal(MySqlDataReader reader, string columnName)
    {
        return reader.GetDecimal(reader.GetOrdinal(columnName));
    }

    protected static decimal GetNullableDecimal(MySqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
    }
}
