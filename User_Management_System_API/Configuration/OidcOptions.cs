using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User_Management_System_API.Configuration
{
    public class OidcOptions
    {
        public string Audience { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
