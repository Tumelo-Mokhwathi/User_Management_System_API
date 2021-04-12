using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User_Management_System_API.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string name, string subject, string message);
    }
}
