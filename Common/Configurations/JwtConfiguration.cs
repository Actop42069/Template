namespace Common.Configurations
{
    public class JwtConfiguration
    {
        public const string SECTION_NAME = "JWT";
        public string Key { get; set; }
        public int ExpireInMinutes { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
    }
}
