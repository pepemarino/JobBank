using JobBank.Data;
using JobBank.EtlConsole.Model;
using JobBank.Management.Interview;
using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JobBank.EtlConsole.Evaluation
{
    public static class EvaluationsETL
    {
        public static async Task<EtlResultMetadata> InterviewEvaluationETL(string connStr, Microsoft.Extensions.Logging.ILogger logger)
        {
            var migrationName = nameof(EvaluationsETL) + "_InterviewEvaluationETL";
            var etlResult = new EtlResultMetadata
            {
                StartTime = DateTime.Now,
                MigrationName = migrationName
            };

            var optionsBuilder = new DbContextOptionsBuilder<EmploymentBankContext>();
            optionsBuilder.UseSqlServer(connStr);

            using var dbContext = new EmploymentBankContext(optionsBuilder.Options);

            if (await dbContext.DataMigrationHistories.AnyAsync(m => m.MigrationName == migrationName))
            {
                etlResult.Message = $"Migration '{migrationName}' has already been executed. Skipping.";
                etlResult.EndTime = DateTime.Now;
                return etlResult;
            }

            const int BatchSize = 500;
            bool hasMoreRecords = true;

            while (hasMoreRecords)
            {
                var batch = await dbContext.Interviews
                         .Where(e => !e.IsMigrated)
                         .Take(BatchSize)
                         .AsTracking()
                         .ToListAsync();

                if (!batch.Any())
                {
                    hasMoreRecords = false;
                    break;
                }

                using var transaction = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    foreach (var entity in batch)
                    {
                        if (!string.IsNullOrEmpty(entity.Result))
                        {
                            try
                            {
                                var interviewMetadata = JsonSerializer.Deserialize<InterviewMetadata>(entity.Result);

                                if (interviewMetadata?.Evaluations?.Any() == true)
                                {
                                    entity.Evaluations = interviewMetadata.Evaluations
                                        .Select(e => new JobBank.Models.Evaluation
                                        {
                                            InterviewId = entity.Id,
                                            Score = e.Score,
                                            UserId = entity.UserId,
                                            PreviousQuestion = e.PreviousQuestion,
                                            PreviousTopic = e.PreviousTopic,
                                            Weight = e.Weight,
                                            Passed = e.Passed,
                                            Strengths = string.Join(",", e.Strengths ?? new List<string>()),
                                            Gaps = string.Join(",", e.Gaps ?? new List<string>()),
                                            Evidence = e.Evidence,
                                            Confidence = e.Confidence,
                                            CreatedDateUtc = entity.CreatedDateUtc
                                        })
                                        .ToList();
                                }
                            }
                            catch (JsonException jsonEx)
                            {
                                etlResult.Errors.Add($"Failed to deserialize Interview {entity.Id}: {jsonEx.Message}");
                                logger.LogWarning($"Invalid JSON in Interview {entity.Id}: {jsonEx.Message}");                               
                            }
                            entity.IsMigrated = true;
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    etlResult.ProcessedRecords += batch.Count;
                    etlResult.TotalRecords += batch.Count;
                    logger.LogInformation($"[{DateTime.Now:T}] Successfully processed batch of {batch.Count} records. Total: {etlResult.TotalRecords}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    etlResult.Errors.Add($"Error processing batch: {ex.Message}");
                    logger.LogError(ex, "Batch processing failed. Transaction rolled back.");
                }
                finally
                {
                    dbContext.ChangeTracker.Clear();
                }
            }

            etlResult.Message = FinalizeMessage(etlResult);
            
            // Record migration completion
            dbContext.DataMigrationHistories.Add(new DataMigrationHistory
            {
                MigrationName = migrationName,
                AppliedAt = DateTime.Now,
                Remarks = etlResult.Message
            });

            await dbContext.SaveChangesAsync();

            etlResult.EndTime = DateTime.Now;
            return etlResult;
        }

        private static string FinalizeMessage(EtlResultMetadata etlResult)
        {
            return etlResult.Errors.Any() 
                ? $"Migrated Records: {etlResult.ProcessedRecords}, Concluded with {etlResult.Errors.Count} errors"
                : $"Completed successfully. Processed {etlResult.TotalRecords} records.";
        }
    }
}
