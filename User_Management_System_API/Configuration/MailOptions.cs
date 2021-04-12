using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User_Management_System_API.Configuration
{
    public class MailOptions
    {
        public string FromName { get; set; }
        public string FromAddress { get; set; }
        public int Port { get; set; }
        public string Server { get; set; }
        public bool UseSsl { get; set; }
    }
}
