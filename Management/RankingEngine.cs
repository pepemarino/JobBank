using System.Text.RegularExpressions;

namespace JobBank.Management
{
    public class RankingEngine
    {
        public double CalcularRanking(Dictionary<string, string> jobDescriptionSections, List<string> userSkills)
        {
            double totalRanking = 0;

            foreach (var seccion in jobDescriptionSections)
            {
                int multiplier = seccion.Key switch
                {
                    "Mandatory" => 10,
                    "Preferable" => 5,
                    "Desirable" => 4,
                    _ => 1
                };

                // Normalize section text once per loop
                string sectionContent = seccion.Value.ToLower();

                foreach (var rawSkill in userSkills)
                {
                    // FIX 1: Remove the whitespace 'entropy' (The " CI/CD" bug)
                    string skill = rawSkill.Trim().ToLower();
                    if (string.IsNullOrEmpty(skill)) continue;

                    // FIX 2: Better matching for technical symbols (.NET, C#)
                    // We use a boundary check that respects '#' and '.'
                    // Or, for high reliability, use a simple 'Contains' or a specialized Regex
                    string pattern = $@"(?i)(^|[\s,;]){Regex.Escape(skill)}([\s,;]|$)";

                    if (Regex.IsMatch(sectionContent, pattern))
                    {
                        totalRanking += multiplier;

                        // Logging for transparency
                        Console.WriteLine($"[MATCH] '{skill}' in {seccion.Key} (+{multiplier} pts)");
                    }
                }
            }
            return totalRanking;
        }
    }
}
