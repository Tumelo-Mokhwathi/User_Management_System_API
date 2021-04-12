using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User_Management_System_API.DataAccess;
using User_Management_System_API.DataAccess.Models;
using User_Management_System_API.Models;
using User_Management_System_API.Services.Interface;

namespace User_Management_System_API.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<AppUser> GetAll()
        {
            var users = _context.Users.Join(
                _context.UserRoles,
                u => u.Id,
                ur => ur.UserId,
                (u, ur) => new
                {
                    User = u,
                    UserRole = ur
                }).Join(
                _context.Roles,
                u => u.UserRole.RoleId,
                r => r.Id,
                (u, r) => new
                {
                    User = u,
                    Role = r
                }).Select((r) => new AppUser
                {
                    Id = r.User.User.Id,
                    Name = r.User.User.Name,
                    Surname = r.User.User.Surname,
                    Email = r.User.User.Email,
                    ContactNo = r.User.User.PhoneNumber,
                    Disabled = r.User.User.Disabled,
                    Role = r.Role.Name
                }).ToList();

            return users;
        }

        public async Task DisableAsync(string id)
        {
            await UpdateAccountStatus(id, true);
        }

        public async Task EnableAsync(string id)
        {
            await UpdateAccountStatus(id, false);
        }

        private async Task UpdateAccountStatus(string id, bool status)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not Found with Specified ID");
            }

            user.Disabled = status;

            _context.Users.Update(user);
            _context.SaveChanges();
        }
    }
}
