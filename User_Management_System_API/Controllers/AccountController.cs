using User_Management_System_API.ActionResult;
using User_Management_System_API.DataAccess.Models;
using User_Management_System_API;
using User_Management_System_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using User_Management_System_API.Constants;
using User_Management_System_API.DataAccess;
using User_Management_System_API.Services.Interface;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using User_Management_System_API.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace User_Management_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IUserService _userService;
        private readonly OidcOptions _oidcOptions;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              RoleManager<IdentityRole> roleManager,
                              ApplicationDbContext context,
                              IEmailSender emailSender,
                              ISmsSender smsSender,
                              IUserService userService,
                              IOptions<OidcOptions> oidcOptions,
                              IConfiguration configuration
                              )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _context = context;
            _userService = userService;
            _oidcOptions = oidcOptions.Value;
            _configuration = configuration;
        }

        [HttpPost("Users/Create")]
        public async Task<IActionResult> Create(string clientId, Register model)
        {
            var source = $"{Constants.General.SourcePrefixName}.create";
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                Surname = model.Surname,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);

            _context.UserRoles.Add(new IdentityUserRole<string>
             {
                RoleId = Roles.RoleDictionary[model.Role].IdentityRole.Id,
                UserId = user.Id
            });
            _context.SaveChanges();

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                var tokenVerificationUrl = Url.Action("VerifyEmail", "Account",
                        new
                        {
                            Id = user.Id,
                            token = token
                        },
                        Request.Scheme);

                var message = string.Format("Good day,<br/><br/>We have sent you this request to register on User Identity "
                + "in order that you may use the application. Just click this "
                + $"<a href=\"{tokenVerificationUrl}\">Confirm</a> to verify your email and complete the "
                + "process to create your account.<br/><br/>Already have an account? Don't worry. Simply click the register link above "
                + "and your account will be linked to your account automatically.<br/><br/>Regards,<br/><br/>Identity User",
                model.Name, $"/{WebUtility.UrlEncode(clientId)}/{WebUtility.UrlEncode(model.Email)}");

                var name = $"{model.Name + " " + model.Surname}";

                await _emailSender.SendEmailAsync(model.Email, "Confirm your email", name, message).ConfigureAwait(false);
                await _smsSender.SendSmsAsync(model.PhoneNumber, "Your account has been created. Please check your email to confirm!").ConfigureAwait(false);

                return ActionResultGenerator.SuccessResult(HttpStatusCode.Created,
                new
                {
                    message = $"Registration completed for {model.Email}, please verify your email.",
                }, 
                source);
            }

            return ActionResultGenerator.ErrorResult(
                HttpStatusCode.BadRequest, 
                result.Errors.ToList().Select(s => s.Description).FirstOrDefault(), 
                source);
        }

        [HttpPost("Users/Authenticate")]
        public async Task<IActionResult> Authenticate(Login model)
        {
            var source = $"{Constants.General.SourcePrefixName}.authentication";

            var user = await _userManager.FindByNameAsync(model.Email).ConfigureAwait(false);

            if (user == null && !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return ActionResultGenerator.ErrorResult(
                    HttpStatusCode.Unauthorized,
                    "Invalid Login and/or password",
                    source);
            }

            var response = await _signInManager.PasswordSignInAsync(user.Email, model.Password, model.RememberMe, false).ConfigureAwait(false);

            var roles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach(var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_oidcOptions.ClientSecret));

            var token = new JwtSecurityToken(
                issuer: _oidcOptions.Authority,
                audience: _oidcOptions.Audience,
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return ActionResultGenerator.SuccessResult(
                HttpStatusCode.OK, 
                new 
                {
                    signedInUser = user,
                    message = $"{user.Email} is authenticated and authorized",
                    isAuthenticated = response.Succeeded,
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                }, 
                source);
        }

        [HttpPost("Users/Signout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync().ConfigureAwait(false);
            var source = $"{Constants.General.SourcePrefixName}.authentication";
            try
            {
                return ActionResultGenerator.SuccessResult(
                    HttpStatusCode.OK,
                    new
                    {
                        message = "User successfully signed out"                     
                    }, 
                    source);
            }
            catch (Exception ex)
            {
                return ActionResultGenerator.ErrorResult(HttpStatusCode.Unauthorized, ex.Message, source);
            }
        }

        [HttpPost("Users/ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string clientId, ForgotPassword model)
        {
            var user = await _userManager.FindByIdAsync(model.Email).ConfigureAwait(false);

            var source = $"{Constants.General.SourcePrefixName}.forgotpassword";

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);

            var message = string.Format("Good day,<br/><br/> "
                    + "Your password resend code has been recieved. "
                    + $"Copy the Code <a href=\"{callbackUrl}\">Code</a> to reset the password."
                    + "<br/><br/>Regards,<br/><br/>Identity User",
                    model.Email, $"/{WebUtility.UrlEncode(clientId)}/{WebUtility.UrlEncode(model.Email)}");

            try
            {
                await _emailSender.SendEmailAsync(model.Email, "name", "Reset Password Code", message).ConfigureAwait(false);

                return ActionResultGenerator.SuccessResult(
                HttpStatusCode.OK,
                new
                {
                    message = "Reset password code has been sent succesfully",
                },
                source);
            }
            catch (Exception ex)
            {
                return ActionResultGenerator.ErrorResult(
                    HttpStatusCode.NotFound,
                    $"Password resend code cannot be send for {user.Email}" + ex.Message,
                    source);
            }
        }

        [HttpPost("Users/ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            var user = await _userManager.FindByIdAsync(model.Email).ConfigureAwait(false);
            var source = $"{Constants.General.SourcePrefixName}.resetpassword";

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password).ConfigureAwait(false);

            return ActionResultGenerator.SuccessResult(
                HttpStatusCode.OK,
                new
                {
                    message = $"Please reset your password by clicking here: <a href='{result}'>link</a>",
                },
                source);
        }

        [HttpGet("Users/ConfirmEmail")]
        public async Task<IActionResult> VerifyEmail(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
            var source = $"{Constants.General.SourcePrefixName}.emailverification";
            var response = await _userManager.ConfirmEmailAsync(user, token).ConfigureAwait(false);

            try
            {
                return ActionResultGenerator.SuccessResult(
                HttpStatusCode.Created,
                new
                {
                    message = $"{user.Email} is verified",
                    isUserVerified = response.Succeeded,
                },
                source);
            }
            catch (Exception ex)
            {
                return ActionResultGenerator.ErrorResult(
                    HttpStatusCode.Forbidden,
                    $"{user.Email} cannot be verified" + ex.Message,
                    source);
            }
        }
    }
}
