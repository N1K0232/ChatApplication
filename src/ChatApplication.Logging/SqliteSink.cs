using System.Data;
using Microsoft.Data.Sqlite;
using Serilog.Core;
using Serilog.Events;

namespace ChatApplication.Logging;

public class SqliteSink : ILogEventSink
{
    private readonly string databasePath;
    private readonly string tableName;

    public SqliteSink(string databasePath, string tableName)
    {
        this.databasePath = databasePath;
        this.tableName = tableName;

        using var connection = CreateSqlConnection();
        CreateSqlTable(connection);
    }

    public void Emit(LogEvent logEvent)
    {
        using var connection = CreateSqlConnection();
        using var command = connection.CreateCommand();

        command.CommandText = $"INSERT INTO {tableName} (Message, Level, TimeStamp, Exception) " +
            "VALUES (@message, @level, @timestamp, @exception)";

        CreateParameter(command, "@message", DbType.String, logEvent.RenderMessage());
        CreateParameter(command, "@level", DbType.String, logEvent.Level.ToString());
        CreateParameter(command, "@timestamp", DbType.DateTime, logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        CreateParameter(command, "@exception", DbType.String, logEvent.Exception?.ToString());

        command.ExecuteNonQuery();
    }

    private void CreateSqlTable(SqliteConnection connection)
    {
        var columnDefinitions = "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "Message TEXT, " +
            "Level VARCHAR(10), " +
            "TimeStamp TEXT, " +
            "Exception TEXT";

        using var command = connection.CreateCommand();

        command.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnDefinitions})";
        command.ExecuteNonQuery();
    }

    private SqliteConnection CreateSqlConnection()
    {
        var builder = new SqliteConnectionStringBuilder { DataSource = databasePath };
        var connection = new SqliteConnection(builder.ConnectionString);

        connection.Open();
        return connection;
    }

    private static void CreateParameter(IDbCommand command, string name, DbType type, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;

        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }
}