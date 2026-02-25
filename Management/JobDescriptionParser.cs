namespace JobBank.Management
{
    using JobBank.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Still a very basic implementation, but it can be improved in the future to detect more sections and be more robust to different formats of job descriptions.
    /// Should be able to detect sections like "Requirements", "Responsibilities", "Desirable", etc. and categorize the text accordingly.
    /// Is LLM better at this than a regex-based approach? Maybe, but for now we can start with this and improve it later if needed.
    /// </summary>
    public class JobDescriptionParser
    {
        // Pattern to detect headings (ignores uppercase/lowercase)
        // Searches for words like Requirements, Required, Plus, Desirable, etc.
        private const string SectionPattern = @"(?i)(?<=\n|^)\s*(?:what you(?:’|')ll\s+do|what you\s+bring|technologies|requirements|mandatory|experience|responsibilities|plus|desirable|valuable|optional|profile)\b\s*(?::|-)?";

        public Dictionary<string, string> GetSections(string jobText)
        {
            var sections = new Dictionary<string, string>();

            // divide the text, keeping the delimiter to know which section it is
            string[] parts = Regex.Split(jobText, SectionPattern);

            // Especial casr: if parts[0] is not empty, add this to GENERAL
            // This is the text before the first section heading, which could be a general description of the job
            // Take a look in the unit test we have examples of this case
            if (!string.IsNullOrWhiteSpace(parts[0]))
            {
                sections["General"] = parts[0];
            }

            // parts[0] could be the general description before any section,
            // put it in a "General" category
            // when i is odd, it's a header
            // when i is even, it's the content of the previous header.  This makes more sense when looking at the SectionPattern
            for (int i = 1; i < parts.Length; i += 2)
            {
                string header = parts[i].Trim().ToLower();
                string content = (i + 1 < parts.Length) ? parts[i + 1] : "";

                string category = MapToCategory(header);

                // Si la categoría ya existe (ej. dos bloques de "Requisitos"), sumamos el texto
                if (sections.ContainsKey(category))
                    sections[category] += " " + content;
                else
                    sections[category] = content;
            }

            return sections;
        }

        private string MapToCategory(string header)
        {
            // Use Contains to handle "Work Experience", "Technical Requirements", etc.
            if (header!.ContainsAny("mandatory", "must", "requirements", "proficient", "bring", "experience"))
                return "Mandatory";

            if (header!.ContainsAny("do", "responsibilities", "role"))
                return "Desirable";

            if (header!.ContainsAny("plus", "valuable", "desired", "optional"))
                return "Preferable";

            return "General";
        }
    }
}
