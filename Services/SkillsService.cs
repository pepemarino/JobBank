using JobBank.Data;
using JobBank.Exceptions;
using JobBank.Models;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Services
{
    public class SkillsService : ISkillsService
    {
        private readonly IDbContextFactory<EmploymentBankContext> _factory;
        private readonly ILogger<ISkillsService> _logger;

        public SkillsService(IDbContextFactory<EmploymentBankContext> dbFactory, ILogger<ISkillsService> logger)
        {
            _factory = dbFactory;
            _logger = logger;
        }

        /// <summary>
        /// Remember this: There is going to be a relationship User 1 to 0..1 Skill Set
        /// Remember this: Skill Set is a string where skills are separated by a comma
        /// REMEMBER THIS: This search will be done by user Id.  Because we do not have a user yet
        ///                This is going to be SingleOrDefault().  If Invalid Operation Exception is thrown
        ///                You know that a mistake was done. This is impoertant because the MATCH will depend on this
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<UserSkillsDTO?> GetUserSkillsAsync(string userId)
        {
            try
            {
                await using var context = await _factory.CreateDbContextAsync();
                var dto = await context.UserSkills
                    .AsNoTracking()
                    .Where(u => u.UserId == userId)
                    .Select(u => new UserSkillsDTO
                    {
                        UserId = u.UserId,
                        RawSkills = u.RawSkills,
                        Version = u.Version,
                        UpdatedAt = u.UpdatedAt
                    })
                    .SingleOrDefaultAsync();

                return dto;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Data integrity issue: Multiple skill sets found for user {UserId}", userId);

                // Rethrow a custom business exception that the Blazor UI can understand
                throw new DataIntegrityException("A system error occurred. Please contact support.", ex);
            }
        }

        /// <summary>
        /// Entity are small, but perhaps AutoMapper is needed
        /// </summary>
        /// <param name="userSkill"></param>
        /// <returns></returns>
        public async Task UpdateOrAddUserSkillsAsync(UserSkillsDTO userSkill)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var skills = await context.UserSkills.SingleOrDefaultAsync(s => s.UserId == userSkill.UserId);

            if (skills == null)
            {
                skills = new UserSkills
                {
                    UserId = userSkill.UserId,
                    CreatedAt = DateTime.UtcNow,
                    RawSkills = userSkill.RawSkills,
                    Version = userSkill.Version
                };
                context.UserSkills.Add(skills);
            }
            else
            {
                skills.UpdatedAt = DateTime.UtcNow;
            }

            skills.RawSkills = userSkill.RawSkills;
            skills.Version = userSkill.Version;

            await context.SaveChangesAsync();
            _logger.LogInformation("User skills for user {UserId} have been updated/added successfully.", userSkill.UserId);
        }
    }
}
