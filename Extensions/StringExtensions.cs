namespace JobBank.Extensions
{
    public static class StringExtensions
    {
        public static string TruncateByLength(this string input, int? maxLength = null)
        {
            var defaultLength = 50; // Default length if not provided
            var length = maxLength.HasValue 
                ? maxLength.Value < 0 ? defaultLength : maxLength.Value // Default length if not provided
                : defaultLength;

            if (string.IsNullOrEmpty(input))            
                return string.Empty;
            
            return input.Length <= length ? input : input.Substring(0, length) + "...";
        }
    }
}
