using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User_Management_System_API.DataAccess.Models;

namespace User_Management_System_API.DataAccess.Configurations
{
    public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
    {
        internal const string ProductUserId = "15c15929-3611-438d-93c5-c8fa94c46a29";
        private const string DefaultUser = "email@outlook.com";
        private const string DefaultPassword = "D3F@ultPa$$w0rd";

        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            var hasher = new PasswordHasher<IdentityUser>();

            builder.HasData(
                new ApplicationUser
                {
                    Id = ProductUserId,
                    UserName = DefaultUser,
                    Email = DefaultUser,
                    EmailConfirmed = true,
                    PasswordHash = hasher.HashPassword(null, DefaultPassword),
                    Disabled = false
                }
            );
        }
    }
}
