namespace JobBank.Management
{
    using JobBank.Extensions;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public partial class JobDescriptionParser
    {       
        public Dictionary<string, string> GetSections(string jobText)
        {
            var sections = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(jobText))
                return new();

            string normalized = jobText.StringNormalize();

            string currentSection = "General";

            foreach (string rawLine in normalized.Split(['\n',':'])) // this smells
            {
                string line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string? detectedSection = DetectSection(line);

                if (detectedSection != null)
                {
                    currentSection = detectedSection;

                    if (!sections.ContainsKey(currentSection))
                        sections[currentSection] = new();

                    continue;
                }

                if (!sections.ContainsKey(currentSection))
                    sections[currentSection] = new();

                sections[currentSection].Add(line);
            }

            return sections.ToDictionary(
                kvp => kvp.Key,
                kvp => string.Join("\n", kvp.Value));
        }

        private string DetectSection(string line)
        {
            string normalized = line
                .Trim()
                .TrimEnd(':')
                .ToLowerInvariant();

            foreach (var category in HeaderMap)
            {
                foreach (var keyword in category.Value)
                {
                    if (normalized.Contains(keyword))
                        return category.Key;
                }
            }

            return null;
        }
    }
}
