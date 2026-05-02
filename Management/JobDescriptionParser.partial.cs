namespace JobBank.Management
{
    public partial class JobDescriptionParser
    {
        private static readonly Dictionary<string, string[]> HeaderMap = new()
        {
            ["Mandatory"] = new[]
            {
                "requirements",
                "must have",
                "required",
                "minimum qualifications",
                "what you bring"
            },

            ["Preferable"] = new[]
            {
                "nice to have",
                "preferred",
                "bonus",
                "plus",
                "desirable",
                "optional"
            },

            ["Responsibilities"] = new[]
            {
                "responsibilities",
                "what you'll do",
                "duties",
                "role"
            },

            ["Technology"] = new[]
            {
                "technologies",
                "tech stack",
                "tools"
            }
        };
    }

    public class ParsedSection
    {
        public string? Category { get; set; }
        public double? Confidence { get; set; }
        public string? Content { get; set; }
    }

    public class Requirement
    {
        public string? Skill { get; set; }

        public string? Evidence { get; set; }
    }
}
