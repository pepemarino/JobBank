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
        private readonly EmploymentBankContext _context;

        public SkillsService(IDbContextFactory<EmploymentBankContext> dbFactory)
        {
            // NOTE ON VIRTUAL ENTITY NAVIGATION:  Context stays alive for the life of the Service to support virtual props
            _context = dbFactory.CreateDbContext();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
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
        public async Task<UserSkillsDTO> GetUserSkillsWithLazyPropsAsync(int userId)
        {            
            try
            {
                return await _context.UserSkills
                    .AsNoTracking()
                    .Where(u => true) //u.UserId == userId)             // Where the guacamole changes
                    .Select(u => new UserSkillsDTO 
                    {
                        UserId = u.UserId,
                        RawSkills = u.RawSkills,
                        Version = u.Version,
                        UpdatedAt = u.UpdatedAt
                    })
                    .SingleOrDefaultAsync();
            }
            catch (InvalidOperationException ex)
            {
                // NEED TO ADD LOGGING:  Log the technical details for your dev team

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
            var skills = await _context.UserSkills.SingleOrDefaultAsync(s => true);// s.UserId == userSkill.UserId);  // it shouild be one in the db 
            if (skills == null)
            {
                // we are adding
                skills = new UserSkills
                {
                    UserId = userSkill.UserId,
                    CreatedAt = DateTime.UtcNow,
                    RawSkills = userSkill.RawSkills,
                    Version = userSkill.Version
                };
                _context.UserSkills.Add(skills);
            }
            else
            {
                skills.UpdatedAt = DateTime.UtcNow;
            }

            skills.RawSkills = userSkill.RawSkills;
            skills.Version = userSkill.Version;

            await _context.SaveChangesAsync();
        }
    }
}
