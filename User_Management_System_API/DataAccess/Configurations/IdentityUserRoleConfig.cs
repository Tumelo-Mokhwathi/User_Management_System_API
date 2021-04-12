using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User_Management_System_API.Constants;

namespace User_Management_System_API.DataAccess.Configurations
{
    public class IdentityUserRoleConfig : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder.HasData(new IdentityUserRole<string>
            {
                RoleId = Roles.RoleDictionary[Roles.SuperAdministrator].IdentityRole.Id,
                UserId = ApplicationUserConfig.ProductUserId
            });
        }
    }
}
