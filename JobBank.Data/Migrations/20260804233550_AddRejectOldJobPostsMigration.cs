using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBank.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectOldJobPostsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing procedure if present
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.spRejectOldJobPosts', 'P') IS NOT NULL
                    DROP PROCEDURE dbo.spRejectOldJobPosts;
            ");

            // Create procedure
            migrationBuilder.Sql(@"
                CREATE PROCEDURE dbo.spRejectOldJobPosts
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    DECLARE @TotalRejected INT = 0;

                    DECLARE @RejectedRecords TABLE (
                        JobId INT,
                        UserId NVARCHAR(128),
                        RejectionTimestamp DATETIME2
                    );

                    WHILE 1 = 1
                    BEGIN
                        DECLARE @Now DATETIME2 = SYSUTCDATETIME();
                        DECLARE @BatchCount INT = 0;

                        DELETE FROM @RejectedRecords;

                        BEGIN TRY
                            BEGIN TRANSACTION;

                            WITH Batch AS (
                                SELECT TOP (2000)
                                    ApplicationDeclined,
                                    AutomaticallyRejected,
                                    ResponseDate,
                                    Timestamp,
                                    Comments,
                                    Id,
                                    UserId
                                FROM JobPost WITH (READPAST)
                                WHERE ApplicationDeclined = 0
                                  AND AutomaticallyRejected = 0
                                  AND ApplicationDate < DATEADD(month, -1, @Now)
                            )
                            UPDATE Batch
                            SET ApplicationDeclined = 1,
                                AutomaticallyRejected = 1,
                                ResponseDate = @Now,
                                Timestamp = @Now,
                                Comments = 'Your application has been automatically rejected due to inactivity for over a month.'
                            OUTPUT INSERTED.Id, INSERTED.UserId, @Now
                            INTO @RejectedRecords (JobId, UserId, RejectionTimestamp);

                            SET @BatchCount = @@ROWCOUNT;

                            IF @BatchCount = 0
                            BEGIN
                                COMMIT TRANSACTION;
                                BREAK;
                            END

                            INSERT INTO RejectionEvents (
                                JobId,
                                UserId,
                                TerminationReason,
                                Timestamp,
                                EventDate,
                                IsProcessed
                            )
                            SELECT JobId, UserId, 'Automatic', RejectionTimestamp, RejectionTimestamp, 0
                            FROM @RejectedRecords;

                            COMMIT TRANSACTION;

                            SET @TotalRejected = @TotalRejected + @BatchCount;

                        END TRY
                        BEGIN CATCH
                            IF @@TRANCOUNT > 0
                                ROLLBACK TRANSACTION;

                            THROW;
                        END CATCH

                        WAITFOR DELAY '00:00:00.050';
                    END

                    SELECT @TotalRejected AS TotalRejected;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.spRejectOldJobPosts', 'P') IS NOT NULL
                    DROP PROCEDURE dbo.spRejectOldJobPosts;
            ");
        }
    }
}
