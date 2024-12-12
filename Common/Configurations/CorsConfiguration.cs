using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Configurations
{
    public class CorsConfiguration
    {
        public const string SECTION_NAME = "CORS";
        public string[] ExposedHeaders { get; set; }
        public string[] AllowedOrigins { get; set; }
    }
}
