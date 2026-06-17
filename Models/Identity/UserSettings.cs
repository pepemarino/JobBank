using JobBank.ModelConfiguration;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Models.Identity
{
    [EntityTypeConfiguration(typeof(UserSettingsConfiguration))]
    public class UserSettings
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public bool ForceMyKey { get; set; } = false; // this setting is currently a duplicate because it is also stored in JobBankUser. It will be removed from JobBankUser in a future refactor.
        public bool UseTutorMode { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
