using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User_Management_System_API.Constants
{
    public class Roles
    {
        public const string SuperAdministrator = "SuperAdministrator";
        public const string Administrator = "Administrator";
        public const string Base = "Base";

        public string[] AllowedToCreate { get; set; }
        public IdentityRole IdentityRole { get; set; }

        public static Dictionary<string, Roles> RoleDictionary { get; } = new Dictionary<string, Roles>()
        {
            {
                Administrator,
                new Roles
                {
                    AllowedToCreate = new string []
                    {
                        Administrator,
                        Base
                    },
                    IdentityRole = new IdentityRole
                    {
                        Id = "c3d7797c-198e-44a8-b7e9-aee659062b43",
                        Name = Administrator
                    }
                }
            },
            {
                Base,
                new Roles
                {
                    AllowedToCreate = new string [0],
                    IdentityRole = new IdentityRole
                    {
                        Id = "1c6a9428-9d52-4942-882b-b1915e8ba523",
                        Name = Base
                    }
                }
            },
            {
                SuperAdministrator,
                new Roles
                {
                    AllowedToCreate = new string []
                    {
                        Administrator,
                        Base,
                        SuperAdministrator
                    },
                    IdentityRole =  new IdentityRole
                    {
                        Id = "8ba793b4-ec94-4397-9218-a83c2e089823",
                        Name = SuperAdministrator
                    }
                }
            }
        };
    }
}
