using System;
using Core.CrossCuttingConcerns.Logging.Serilog.ConfigurationModels;
using Core.Utilities.IoC;
using Core.Utilities.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace Core.CrossCuttingConcerns.Logging.Serilog.Loggers
{

    public class MsSqlLogger : LoggerServiceBase
    {
        public MsSqlLogger()
        {
            var configuration = ServiceTool.ServiceProvider.GetService<IConfiguration>();

            var logConfig = configuration.GetSection("SeriLogConfigurations:MsSqlConfiguration")
                                .Get<MsSqlConfiguration>() ??
                            throw new Exception(SerilogMessages.NullOptionsMessage);
            var sinkOpts = new MSSqlServerSinkOptions
            {
                TableName = "Logs",
                AutoCreateSqlTable = true,
                BatchPostingLimit = 1, // <--- 1 Log bile olsa bekleme, hemen yaz!
                BatchPeriod = TimeSpan.FromSeconds(1) // <--- Maksimum 1 saniye bekle
            };

            var columnOpts = new ColumnOptions();

            columnOpts.Store.Remove(StandardColumn.Message);
            columnOpts.Store.Remove(StandardColumn.Properties);

            var seriLogConfig = new LoggerConfiguration()
                .WriteTo.MSSqlServer(
                    connectionString: logConfig.ConnectionString,
                    sinkOptions: sinkOpts,
                    columnOptions: columnOpts) // <-- columnOpts'u buraya verdiğinden emin ol
                .CreateLogger();

           
            Logger = seriLogConfig;
        }
    }
}