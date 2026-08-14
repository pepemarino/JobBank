using JobBank.Data;
using JobBank.EventHandler.EventArgs;
using JobBank.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBank.EventHandler.Handlers
{
    public class RejectionEventHandler
    {
        private readonly IDbContextFactory<EmploymentBankContext> _dbContextFactory;

        public RejectionEventHandler(IDbContextFactory<EmploymentBankContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task HandleRejectionAsync(RejectionEventArg e)
        {
            using var context = _dbContextFactory.CreateDbContext();

            var rejectionEvent = new RejectionEvents
            {
                JobId = e.JobPostId,
                UserId = e.UserId,
                TerminationReason = e.TerminationReason
            };

            context.RejectionEvents.Add(rejectionEvent);
            await context.SaveChangesAsync();            
        }
    }
}
