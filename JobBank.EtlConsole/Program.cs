using JobBank.EtlConsole.Evaluation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSerilog();
});

ILogger logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("=== Starting One-Time ETL Migration ===");

var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__EmploymentBankContext");
if (string.IsNullOrWhiteSpace(connStr))
{
    logger.LogError("Connection string 'EmploymentBankContext' not found. Set environment variable 'ConnectionStrings__EmploymentBankContext'.");
    throw new InvalidOperationException(
        "Connection string 'EmploymentBankContext' not found. Set environment variable 'ConnectionStrings__EmploymentBankContext'.");
}

#region ETL Execution

var etlResult = await EvaluationsETL.InterviewEvaluationETL(connStr, logger);

#endregion ETL Execution

logger.LogInformation($"=== ETL Finished Successfully. Total Migrated: {etlResult.TotalRecords} records in {(etlResult.EndTime - etlResult.StartTime).TotalSeconds:F2}s ===");

if (etlResult.Errors.Any())
{
    logger.LogWarning($"Completed with {etlResult.Errors.Count} errors: {string.Join("; ", etlResult.Errors)}");
}

await Log.CloseAndFlushAsync();