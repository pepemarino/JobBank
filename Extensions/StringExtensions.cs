using System.Text.RegularExpressions;

namespace JobBank.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// This is the hash for the canonical form of the job description,
        /// used i the JobAnalysis table to avoid duplicate analyses.
        /// </summary>
        /// <param name="jodDescription"></param>
        /// <returns></returns>
        public static string ToCanonicalHash(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return string.Empty;

            // Strip HTML
            string clean = Regex.Replace(str, "<.*?>", string.Empty);

            // This pattern REMOVES C++, REMOVES non-NET dots, and REMOVES non-alphanumerics (except #)
            string pattern = @"C\+\+|(?<!\.)\.(?!NET)|[^a-zA-Z0-9\s#.]";
            clean = Regex.Replace(clean, pattern, "", RegexOptions.IgnoreCase);

            var words = clean.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string[] stopWords = { "the", "and", "a", "of", "to", "in", "is" };
            words = words.Where(w => !stopWords.Contains(w)).ToArray();

            string essence = string.Join(" ", words);

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(essence));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
