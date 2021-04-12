using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User_Management_System_API.Models
{
    public class AppUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public bool Disabled { get; set; }
        public string Role { get; set; }
    }
}
