namespace PlexLightSwitcher.Models
{
    public class NeviWebUser
    {
        public User User { get; set; }
        public Account Account { get; set; }
        public long Iat { get; set; }
        public string PermissionContext { get; set; }
        public string Session { get; set; }
        public string RefreshToken { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Locale { get; set; }
        public Format Format { get; set; }
        public int Initialized { get; set; }
        public int DataPolicyConsent { get; set; }
    }

    public class Account
    {
        public int Id { get; set; }
        public string Interface { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string BetaLevel { get; set; }
    }

    public class Format
    {
        public string Time { get; set; }
        public string Temperature { get; set; }
    }
}
