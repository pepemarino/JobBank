using System.Text.RegularExpressions;

namespace JobBank.Management
{
    public class RankingEngine
    {
        private readonly ILogger<RankingEngine> _logger;

        public RankingEngine(ILogger<RankingEngine> logger)
        {
            _logger = logger;
        }

        public double CalcularRanking(Dictionary<string, string> jobDescriptionSections, List<string> userSkills)
        {
            double totalRanking = 0;
            HashSet<string> matchedSkills = new();

            foreach (var seccion in jobDescriptionSections)
            {
                int multiplier = seccion.Key switch
                {
                    "Mandatory" => 10,
                    "Technology" => 8,
                    "Preferable" => 7,
                    _ => 1
                };

                // Normalize section text once per loop
                string sectionContent = seccion.Value.ToLower();

                foreach (var rawSkill in userSkills)
                {
                    string skill = rawSkill.Trim().ToLower();
                    if (string.IsNullOrEmpty(skill)) continue;

                    // We use a boundary check that respects '#' and '.'
                    // Or, for high reliability, use a simple 'Contains' or a specialized Regex
                    string pattern = $@"(?i)(^|[\s,;]){Regex.Escape(skill)}([\s,;]|$)";

                    if (matchedSkills.Contains(skill))
                    {
                        _logger.LogInformation($"[SKIP] '{skill}' already matched in a previous section, skipping duplicate match.");
                        continue; // Skip if we've already matched this skill in a previous section
                    }

                    if (Regex.IsMatch(sectionContent, pattern))
                    {
                        totalRanking += multiplier;
                        matchedSkills.Add(skill);

                        // Logging for transparency
                        _logger.LogInformation($"[MATCH] '{skill}' in {seccion.Key} (+{multiplier} pts)");
                    }
                    else
                    {
                        // Logging for transparency
                        _logger.LogInformation($"[NO MATCH] '{skill}' not found in {seccion.Key}");
                    }
                }
            }
            return totalRanking;            
        }
    }
}
