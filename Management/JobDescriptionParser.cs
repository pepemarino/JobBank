namespace JobBank.Management
{
    using JobBank.Extensions;
    using System.Collections.Generic;

    /// <summary>
    /// This parser has become a bit of a Frankenstein's monster, and it is not pretty. 
    /// It has tricks and traps. this is a problem; a true hack job.
    /// It is designed to handle the wide variety of formats that job descriptions can come in, 
    /// and to extract the relevant sections for analysis by the TrainerAssistant.
    /// It a lexical tokenizer that looks for common section headers 
    /// (like "Responsibilities", "Qualifications", etc.) and splits the job description into sections based on those headers.
    /// It increases the need to implement #73
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

            foreach (string rawLine in normalized.Split('\n'))
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

            return sections
                .Where(kvp => kvp.Value.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => string.Join("\n", kvp.Value));
        }

        private string? DetectSection(string line)
        {
            string normalized = line
                .Trim()
                .TrimEnd(':')
                .ToLowerInvariant();

            foreach (var category in HeaderMap)
            {
                foreach (var keyword in category.Value)
                {
                    if (normalized.Equals(keyword))
                        return category.Key;
                }
            }

            return null;
        }
    }
}
