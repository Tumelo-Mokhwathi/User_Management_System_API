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
    public class IdentityRoleConfig : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            var data = Roles.RoleDictionary
                         .Select(role => role.Value.IdentityRole)
                         .ToList();

            builder.HasData(data);
        }
    }
}
