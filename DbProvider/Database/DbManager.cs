using System.Data;
using Microsoft.Data.SqlClient;

namespace DbProvider.Database;

public class DbManager : IDbManager
{
    private readonly SqlConnection _connection;

    public DbManager(IDatabaseConnectionDetails connectionDetails)
    {
        var connectionString = $"Data Source={connectionDetails.DataSource};Initial Catalog={connectionDetails.DatabaseName};User ID={connectionDetails.Username};Password={connectionDetails.Password};MultipleActiveResultSets=True;TrustServerCertificate=True;";
        _connection = new SqlConnection(connectionString);
    }
    
    public async Task Open()
    {
        await _connection.OpenAsync();
    }

    public async Task<bool> InsertAsync(string tableName, params KeyValuePair<string, object>[] values)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();
        string columns = string.Join(", ", values.Select(v => v.Key));
        string parameters = string.Join(", ", values.Select(v => $"@{v.Key}"));
        
        var query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";
        
        var command = new SqlCommand(query, _connection);
        foreach (var parameter in values)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
            {
                command.Parameters.Add(p);
            }
        }
        
        int rowsAffected = await command.ExecuteNonQueryAsync();
        
        return rowsAffected > 0;
    }

    public async Task<T?> InsertAsyncWithReturn<T>(string tableName, string outputColumn, params KeyValuePair<string, object>[] values)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();
        string columns = string.Join(", ", values.Select(v => v.Key));
        string parameters = string.Join(", ", values.Select(v => $"@{v.Key}"));
        
        var query = $"INSERT INTO {tableName} ({columns}) output inserted.{outputColumn} VALUES ({parameters})";

        var command = new SqlCommand(query, _connection);
        foreach (var parameter in values)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
            {
                command.Parameters.Add(p);
            }
        }

        var result = await command.ExecuteScalarAsync();

        if (result is null)
        {
            return default;
        }
        
        return (T)result;
    }
    
    public bool Insert(string tableName, params KeyValuePair<string, object>[] values)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            _connection.Open();
        string columns = string.Join(", ", values.Select(v => v.Key));
        string parameters = string.Join(", ", values.Select(v => $"@{v.Key}"));
        string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";
        
        var command = new SqlCommand(query, _connection);
        foreach (var parameter in values)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
            {
                command.Parameters.Add(p);
            }
        }
        
        int rowsAffected = command.ExecuteNonQuery();
        
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateAsync(string tableName, KeyValuePair<string, object> matchKey, params KeyValuePair<string, object>[] values)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
           await _connection.OpenAsync();
        string setValues = string.Join(", ", values.Select(v => $"{v.Key} = @{v.Key}"));
        string query = $"UPDATE {tableName} SET {setValues} WHERE {matchKey.Key} = @{matchKey.Key}";
        
        var command = new SqlCommand(query, _connection);
        foreach (var parameter in values)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
            {
                command.Parameters.Add(p);
            }
        }
        
        command.Parameters.Add(CreateParameter(matchKey));
        
        int rowsAffected = await command.ExecuteNonQueryAsync();
        
        return rowsAffected > 0;
    }

    public bool Update(string tableName, KeyValuePair<string, object> matchKey, params KeyValuePair<string, object>[] values)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            _connection.Open();
        string setValues = string.Join(", ", values.Select(v => $"{v.Key} = @{v.Key}"));
        string query = $"UPDATE {tableName} SET {setValues} WHERE {matchKey.Key} = @{matchKey.Key}";
        
        var command = new SqlCommand(query, _connection);
        foreach (var parameter in values)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
            {
                command.Parameters.Add(p);
            }
        }
        
        command.Parameters.Add(CreateParameter(matchKey));
        
        int rowsAffected = command.ExecuteNonQuery();
        
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(string tableName, params KeyValuePair<string, object>[] properties)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();
        string whereClause = string.Join(" AND ", properties.Select(p => $"{p.Key} = @{p.Key}"));
        string query = $"DELETE FROM {tableName} WHERE {whereClause}";
        
        var command = new SqlCommand(query, _connection);
        foreach (var parameter in properties)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
            {
                command.Parameters.Add(p);
            }
        }
        
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public bool Delete(string tableName, params KeyValuePair<string, object>[] properties)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            _connection.Open();
        string whereClause = string.Join(" AND ", properties.Select(p => $"{p.Key} = @{p.Key}"));
        string query = $"DELETE FROM {tableName} WHERE {whereClause}";
        
        var command = new SqlCommand(query, _connection);
        foreach (var parameter in properties)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
            {
                command.Parameters.Add(p);
            }
        }
        
        return command.ExecuteNonQuery() > 0;
    }

    public async Task<bool> ContainsAnyAsync(string tableName, params KeyValuePair<string, object>[] properties)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();
        string whereClause = string.Join(" AND ", properties.Select(p => $"{p.Key} = @{p.Key}"));
        string query = $"SELECT COUNT(*) FROM {tableName} WHERE {whereClause}";
        
        var command = new SqlCommand(query, _connection);
        foreach (var parameter in properties)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
            {
                command.Parameters.Add(p);
            }
        }

        object? count = await command.ExecuteScalarAsync();
        if (count is null)
        {
            return false;
        }
        
        return (int)count > 0;
    }

    public bool ContainsAny(string tableName, params KeyValuePair<string, object>[] properties)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            _connection.Open();
        string whereClause = string.Join(" AND ", properties.Select(p => $"{p.Key} = @{p.Key}"));
        string query = $"SELECT COUNT(*) FROM {tableName} WHERE {whereClause}";
        
        var command = new SqlCommand(query, _connection);
        foreach (var parameter in properties)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
            {
                command.Parameters.Add(p);
            }
        }

        object? count = command.ExecuteScalar();
        if (count is null)
        {
            return false;
        }
        
        return (int)count > 0;
    }

    /// <summary>
    ///     Stop the connection to the SQL Database
    /// </summary>
    /// <returns></returns>
    public async void Stop()
    {
        await _connection.CloseAsync();
    }


    /// <summary>
    ///     Execute a custom query
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The number of rows that the query modified</returns>
    public async Task<int> ExecuteAsync(string query)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();
        var command = new SqlCommand(query, _connection);
        var answer  = await command.ExecuteNonQueryAsync();
        return answer;
    }

    /// <summary>
    ///     Execute a custom query
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The number of rows that the query modified</returns>
    public int Execute(string query)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            _connection.Open();
        var command = new SqlCommand(query, _connection);
        var r       = command.ExecuteNonQuery();

        return r;
    }

    /// <summary>
    ///     Read data from the result table and return the first row
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The result is a string that has all values separated by space character</returns>
    public async Task<string?> ReadDataAsync(string query)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();
        var command = new SqlCommand(query, _connection);
        var reader  = await command.ExecuteReaderAsync();

        var values = new object[reader.FieldCount];
        if (reader.Read())
        {
            reader.GetValues(values);
            return string.Join<object>(" ", values);
        }

        return null;
    }

    /// <summary>
    ///     Read data from the result table and return the first row
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="parameters">The parameters of the query</param>
    /// <returns>The result is a string that has all values separated by space character</returns>
    public async Task<string?> ReadDataAsync(string query, params KeyValuePair<string, object>[] parameters)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();

        var command = new SqlCommand(query, _connection);
        foreach (var parameter in parameters)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
                command.Parameters.Add(p);
        }

        var reader = await command.ExecuteReaderAsync();

        var values = new object[reader.FieldCount];
        if (reader.Read())
        {
            reader.GetValues(values);
            return string.Join<object>(" ", values);
        }

        return null;
    }

    /// <summary>
    ///     Read data from the result table and return the first row
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The result is a string that has all values separated by space character</returns>
    public string? ReadData(string query)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            _connection.Open();
        var command = new SqlCommand(query, _connection);
        var reader  = command.ExecuteReader();

        var values = new object[reader.FieldCount];
        if (reader.Read())
        {
            reader.GetValues(values);
            return string.Join<object>(" ", values);
        }

        return null;
    }

    /// <summary>
    ///     Read data from the result table and return the first row
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The first row as separated items</returns>
    public async Task<object[]?> ReadDataArrayAsync(string query)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();
        var command = new SqlCommand(query, _connection);
        var reader  = await command.ExecuteReaderAsync();

        var values = new object[reader.FieldCount];
        if (reader.Read())
        {
            reader.GetValues(values);
            return values;
        }

        return null;
    }

    public async Task<object[]?> ReadDataArrayAsync(string query, params KeyValuePair<string, object>[] parameters)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();

        var command = new SqlCommand(query, _connection);
        foreach (var parameter in parameters)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
                command.Parameters.Add(p);
        }

        var reader = await command.ExecuteReaderAsync();

        var values = new object[reader.FieldCount];
        if (reader.Read())
        {
            reader.GetValues(values);
            return values;
        }

        return null;

    }
    
    /// <summary>
    ///     Read data from the result table and return the first row
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The first row as separated items</returns>
    public object[]? ReadDataArray(string query)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            _connection.Open();
        var command = new SqlCommand(query, _connection);
        var reader  = command.ExecuteReader();

        var values = new object[reader.FieldCount];
        if (reader.Read())
        {
            reader.GetValues(values);
            return values;
        }

        return null;
    }

    /// <summary>
    ///     Read all rows from the result table and return them as a list of string arrays. The string arrays contain the
    ///     values of each row
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>A list of string arrays representing the values that the query returns</returns>
    public async Task<List<string[]>?> ReadAllRowsAsync(string query)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();
        var command = new SqlCommand(query, _connection);
        var reader  = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
            return null;

        List<string[]> rows = new();
        while (await reader.ReadAsync())
        {
            var values = new string[reader.FieldCount];
            reader.GetValues(values);
            rows.Add(values);
        }

        if (rows.Count == 0) return null;

        return rows;
    }

    /// <summary>
    /// Create a parameter for a query
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    /// <param name="value">The value of the parameter</param>
    /// <returns>The SQLiteParameter that has the name, value and DBType set according to your inputs</returns>
    private static SqlParameter? CreateParameter(string name, object value)
    {
        var parameter = new SqlParameter();
        parameter.ParameterName = name;
        parameter.Value = value;

        if (value is string)
            parameter.DbType = DbType.String;
        else if (value is int)
            parameter.DbType = DbType.Int32;
        else if (value is long)
            parameter.DbType = DbType.Int64;
        else if (value is float)
            parameter.DbType = DbType.Single;
        else if (value is double)
            parameter.DbType = DbType.Double;
        else if (value is bool)
            parameter.DbType = DbType.Boolean;
        else if (value is DateTime)
            parameter.DbType = DbType.DateTime;
        else if (value is byte[])
            parameter.DbType = DbType.Binary;
        else if (value is Guid)
            parameter.DbType = DbType.Guid;
        else if (value is decimal)
            parameter.DbType = DbType.Decimal;
        else if (value is TimeSpan)
            parameter.DbType = DbType.Time;
        else if (value is DateTimeOffset)
            parameter.DbType = DbType.DateTimeOffset;
        else if (value is ushort)
            parameter.DbType = DbType.UInt16;
        else if (value is uint)
            parameter.DbType = DbType.UInt32;
        else if (value is ulong)
            parameter.DbType = DbType.UInt64;
        else if (value is sbyte)
            parameter.DbType = DbType.SByte;
        else if (value is short)
            parameter.DbType = DbType.Int16;
        else if (value is byte)
            parameter.DbType = DbType.Byte;
        else if (value is char)
            parameter.DbType = DbType.StringFixedLength;
        else if (value is char[])
            parameter.DbType = DbType.StringFixedLength;
        else if (value is DBNull)
            parameter.DbType = DbType.Object;
        else
            return null;

        return parameter;
    }


    /// <summary>
    /// Create a parameter for a query. The function automatically detects the type of the value.
    /// </summary>
    /// <param name="parameterValues">The parameter raw inputs. The Key is name and the Value is the value of the parameter</param>
    /// <returns>The SQLiteParameter that has the name, value and DBType set according to your inputs</returns>
    private static SqlParameter? CreateParameter(KeyValuePair<string, object> parameterValues)
    {
        return CreateParameter(parameterValues.Key, parameterValues.Value);
    }

    /// <summary>
    /// Execute a query with parameters
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="parameters">The parameters of the query</param>
    /// <returns>The number of rows that the query modified in the database</returns>
    public async Task<int> ExecuteNonQueryAsync(string query, params KeyValuePair<string, object>[] parameters)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();

        var command = new SqlCommand(query, _connection);
        foreach (var parameter in parameters)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
                command.Parameters.Add(p);
        }

        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Execute a query with parameters that returns a specific type of object. The function will return the first row of the result transformed into the specified type.
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="convertor">The convertor function that will convert each row of the response into an object of <typeparamref name="T"/></param>
    /// <param name="parameters">The parameters of the query</param>
    /// <typeparam name="T">The return object type</typeparam>
    /// <returns>An object of type T that represents the output of the convertor function based on the array of objects that the first row of the result has</returns>
    public async Task<T?> ReadObjectOfTypeAsync<T>(string query, Func<object[], T> convertor, params KeyValuePair<string, object>[] parameters)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();

        var command = new SqlCommand(query, _connection);
        foreach (var parameter in parameters)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
                command.Parameters.Add(p);
        }

        var reader = await command.ExecuteReaderAsync();
        var values = new object[reader.FieldCount];
        if (reader.Read())
        {
            reader.GetValues(values);
            return convertor(values);
        }

        return default;
    }


    /// <summary>
    /// Execute a query with parameters that returns a specific type of object. The function will return a list of objects of the specified type.
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="convertor">The convertor from object[] to T</param>
    /// <param name="parameters">The parameters of the query</param>
    /// <typeparam name="T">The expected object type</typeparam>
    /// <returns>A list of objects of type T that represents each line of the output of the specified query, converted to T</returns>
    public async Task<List<T>> ReadListOfTypeAsync<T>(string query, Func<object[], T> convertor,
        params KeyValuePair<string, object>[] parameters)
    {
        if (!_connection.State.HasFlag(ConnectionState.Open))
            await _connection.OpenAsync();

        var command = new SqlCommand(query, _connection);
        foreach (var parameter in parameters)
        {
            var p = CreateParameter(parameter);
            if (p is not null)
                command.Parameters.Add(p);
        }

        var reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
        {
            return new();
        }

        List<T> rows = new();
        while (await reader.ReadAsync())
        {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);
            rows.Add(convertor(values));
        }

        return rows;
    }
}