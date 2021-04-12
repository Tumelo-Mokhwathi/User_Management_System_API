using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User_Management_System_API.Configuration
{
    public class CorsOptions
    {
        public string AllowedOrigins { get; set; }
        public string[] GetAllowedOriginsAsArray()
        {
            return AllowedOrigins.Split(',').Select(o => o.Trim()).ToArray();
        }
    }
}
