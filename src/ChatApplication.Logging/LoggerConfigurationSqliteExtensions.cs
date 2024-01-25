using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace ChatApplication.Logging;

public static class LoggerConfigurationSqliteExtensions
{
    public static LoggerConfiguration Sqlite(this LoggerSinkConfiguration sinkConfiguration,
        string databasePath, string tableName, IConfiguration configuration,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum, LoggingLevelSwitch? levelSwitch = null)
    {
        var sink = new SqliteSink(databasePath, tableName);
        return sinkConfiguration.Sink(sink, restrictedToMinimumLevel, levelSwitch);
    }
}