namespace DbProvider.Database;

public interface IDbManager
{
    /// <summary>
    ///     Open the SQL Connection. To close use the Stop() method
    /// </summary>
    /// <returns></returns>
    Task Open();
    
    /// <summary>
    ///     Stop the connection to the SQL Database
    /// </summary>
    /// <returns></returns>
    void Stop();

    /// <summary>
    ///     <para>
    ///         Insert into a specified table some values
    ///     </para>
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="values">The values to be inserted</param>
    /// <returns></returns>
    Task<bool> InsertAsync(string tableName, params KeyValuePair<string, object>[] values);

    /// <summary>
    ///     <para>
    ///         Insert into a specified table some values
    ///     </para>
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="outputColumn">The column from which the value should be returned after the insertion</param>
    /// <param name="values">The values to be inserted (in the correct order and number)</param>
    /// <returns>The value of the specified column after the insertion. It is usually the Id. If null, nothing is returned (failed probably at insertion) </returns>
    Task<T?> InsertAsyncWithReturn<T>(string tableName, string outputColumn, params KeyValuePair<string, object>[] values);

    /// <summary>
    ///     <para>
    ///         Insert into a specified table some values
    ///     </para>
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="values">The values to be inserted (in the correct order and number)</param>
    /// <returns></returns>
    bool Insert(string tableName, params KeyValuePair<string, object>[] values);
    
    Task<bool> UpdateAsync(string tableName, KeyValuePair<string, object> matchKey, params KeyValuePair<string, object>[] values);
    bool Update(string tableName, KeyValuePair<string, object> matchKey, params KeyValuePair<string, object>[] values);

    /// <summary>
    ///     Remove every row in a table that has any specified properties
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="properties">The properties to filter the deletion</param>
    /// <returns></returns>
    Task<bool> DeleteAsync(string tableName, params KeyValuePair<string, object>[] properties);

    /// <summary>
    ///     Remove every row in a table that has any specified properties
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="properties">The properties to filter the deletion</param>
    /// <returns></returns>
    bool Delete(string tableName, params KeyValuePair<string, object>[] properties);

    /// <summary>
    ///     Check if the key exists in the table
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="properties">The properties of the search</param>
    /// <returns></returns>
    Task<bool> ContainsAnyAsync(string tableName, params KeyValuePair<string, object>[] properties);

    /// <summary>
    ///     Check if the key exists in the table
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="properties">The properties of the search</param>
    /// <returns></returns>
    bool ContainsAny(string tableName, params KeyValuePair<string, object>[] properties);

    /// <summary>
    ///     Execute a custom query
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The number of rows that the query modified</returns>
    Task<int> ExecuteAsync(string query);

    /// <summary>
    ///     Execute a custom query
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The number of rows that the query modified</returns>
    int Execute(string query);

    /// <summary>
    ///     Read data from the result table and return the first row
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The result is a string that has all values separated by space character</returns>
    Task<string?> ReadDataAsync(string query);

    /// <summary>
    ///     Read data from the result table and return the first row
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="parameters">The parameters of the query</param>
    /// <returns>The result is a string that has all values separated by space character</returns>
    Task<string?> ReadDataAsync(string query, params KeyValuePair<string, object>[] parameters);

    /// <summary>
    ///     Read data from the result table and return the first row
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The result is a string that has all values separated by space character</returns>
    string? ReadData(string query);

    /// <summary>
    ///     Read data from the result table and return the first row
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The first row as separated items</returns>
    Task<object[]?> ReadDataArrayAsync(string query);

    Task<object[]?> ReadDataArrayAsync(string query, params KeyValuePair<string, object>[] parameters);

    /// <summary>
    ///     Read data from the result table and return the first row
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The first row as separated items</returns>
    object[]? ReadDataArray(string query);

    /// <summary>
    ///     Read all rows from the result table and return them as a list of string arrays. The string arrays contain the
    ///     values of each row
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>A list of string arrays representing the values that the query returns</returns>
    Task<List<string[]>?> ReadAllRowsAsync(string query);

    /// <summary>
    /// Execute a query with parameters
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="parameters">The parameters of the query</param>
    /// <returns>The number of rows that the query modified in the database</returns>
    Task<int> ExecuteNonQueryAsync(string query, params KeyValuePair<string, object>[] parameters);

    /// <summary>
    /// Execute a query with parameters that returns a specific type of object. The function will return the first row of the result transformed into the specified type.
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="convertor">The convertor function that will convert each row of the response into an object of <typeparamref name="T"/></param>
    /// <param name="parameters">The parameters of the query</param>
    /// <typeparam name="T">The return object type</typeparam>
    /// <returns>An object of type T that represents the output of the convertor function based on the array of objects that the first row of the result has</returns>
    Task<T?> ReadObjectOfTypeAsync<T>(string query, Func<object[], T> convertor, params KeyValuePair<string, object>[] parameters);

    /// <summary>
    /// Execute a query with parameters that returns a specific type of object. The function will return a list of objects of the specified type.
    /// </summary>
    /// <param name="query">The query to execute</param>
    /// <param name="convertor">The convertor from object[] to T</param>
    /// <param name="parameters">The parameters of the query</param>
    /// <typeparam name="T">The expected object type</typeparam>
    /// <returns>A list of objects of type T that represents each line of the output of the specified query, converted to T</returns>
    Task<List<T>> ReadListOfTypeAsync<T>(string query, Func<object[], T> convertor,
        params KeyValuePair<string, object>[] parameters);
}