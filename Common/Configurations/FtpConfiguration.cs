namespace Common.Configurations
{
    public class FtpConfiguration
    {
        public const string SECTION_NAME = "FTP";

        public string Hostname { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
