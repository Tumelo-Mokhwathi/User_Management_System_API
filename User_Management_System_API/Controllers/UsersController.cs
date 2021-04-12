using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using User_Management_System_API.ActionResult;
using User_Management_System_API.Configuration;
using User_Management_System_API.Constants;
using User_Management_System_API.DataAccess;
using User_Management_System_API.DataAccess.Models;
using User_Management_System_API.Services;
using User_Management_System_API.Services.Interface;

namespace User_Management_System_API.Controllers
{
    [Authorize(Roles = Roles.Administrator + "," + Roles.SuperAdministrator)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        [HttpGet("GetUsers")]
        public IActionResult GetAllUsers()
        {
            var users = _userService.GetAll();

            var source = $"{General.SourcePrefixName}.users";

            try
            {
                return ActionResultGenerator.SuccessResult(HttpStatusCode.OK, users, source);
            }
            catch (Exception ex)
            {
                return ActionResultGenerator.ErrorResult(HttpStatusCode.Unauthorized, ex.Message, source);
            }
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.Id == id);
            var source = $"{Constants.General.SourcePrefixName}.deleteaccount";
            var response = await _userManager.DeleteAsync(user).ConfigureAwait(false);

            try
            {
                return ActionResultGenerator.SuccessResult(
                HttpStatusCode.OK,
                new
                {
                    message = $"{user.Email} is deleted succesfully!",
                    isUserDeleted = response.Succeeded,
                },
                source);
            }
            catch (KeyNotFoundException)
            {
                return ActionResultGenerator.ErrorResult(
                    HttpStatusCode.NotFound,
                    "User not found.",
                    source);
            }
            catch (Exception ex)
            {
                return ActionResultGenerator.ErrorResult(
                    HttpStatusCode.InternalServerError,
                    ex.Message,
                    source);
            }
        }
    }
}
