using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User_Management_System_API.DataAccess.Models;
using User_Management_System_API.Models;

namespace User_Management_System_API.Services.Interface
{
    public interface IUserService
    {
        List<AppUser> GetAll();
        Task DisableAsync(string id);
        Task EnableAsync(string id);
    }
}
