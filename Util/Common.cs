using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobBank.Util
{
    public static class Common
    {
        public static readonly JsonSerializerOptions StrictValidationOptions = new()
        {
            PropertyNameCaseInsensitive = false,
            ReadCommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };
    }
}
