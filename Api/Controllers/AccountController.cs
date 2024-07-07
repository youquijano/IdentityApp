using Api.DTOs.Account;
using Api.Models;
using Api.Service;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JWTService jWTService;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly EmailService emailService;
        private readonly IConfiguration configuration;
        private readonly HttpClient _facebookHttpClient;

        public AccountController(JWTService jWTService,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            EmailService emailService,
            IConfiguration configuration)
        {
            this.jWTService = jWTService;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailService = emailService;
            this.configuration = configuration;
            _facebookHttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com")
            };
        }

        [Authorize]
        [HttpGet("Refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            var user = await userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return await CreateApplicationUserDto(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await userManager.FindByNameAsync(model.Username);

            if (user == null) return Unauthorized("Invalid username or password");

            if (user.EmailConfirmed == false) return Unauthorized("Please confirm your email");

            var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.IsLockedOut) return Unauthorized(string.Format("Your account has been locked. You should wait until {0} (UTC time) to be able to login", user.LockoutEnd));

            if (!result.Succeeded)
            {
                //user input an invalid password
                if (!user.UserName.Equals(SD.AdminUserName))
                {
                    await userManager.AccessFailedAsync(user);
                }
                if (user.AccessFailedCount >= SD.MaximumLoginAttemp)
                {
                    //lock user for one day
                    await userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddDays(1));
                    return Unauthorized(string.Format("Your account has been locked. You should wait until {0} (UTC time) to be able to login", user.LockoutEnd));
                }

                return Unauthorized("Invalid username or password");
            }

            //user inputs a correct password
            await userManager.ResetAccessFailedCountAsync(user);
            await userManager.SetLockoutEndDateAsync(user, null);

            return await CreateApplicationUserDto(user);
        }

        [HttpPost("login-with-third-party")]
        public async Task<ActionResult<UserDto>> LoginWithThirdParty(LoginWithExternalDto model)
        {
            if (model.Provider.Equals(SD.Facebook))
            {
                try
                {
                    if (!FacebookValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unable to login with facebook");
                    }
                }
                catch (Exception e)
                {
                    return Unauthorized();
                }
            }
            else if (model.Provider.Equals(SD.Google))
            {
                try
                {
                    if (!GoogleValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unable to login with Google");
                    }
                }
                catch (Exception e)
                {
                    return Unauthorized("Unable to login with Google");
                }
            }
            else
            {
                return BadRequest("Invalid provider");
            }

            var user = await userManager.Users.FirstOrDefaultAsync(x => x.UserName == model.UserId && x.Provider == model.Provider);
            if (user == null) return Unauthorized("Unable to find your account");

            return await CreateApplicationUserDto(user);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (await CheckEmailExistsAsync(model.Email))
            {
                return BadRequest($"An existing account is using {model.Email}, email address. Please try with another email address");
            }

            var userToAdd = new User
            {
                Email = model.Email.ToLower(),
                FirstName = model.FirstName.ToLower(),
                LastName = model.LastName.ToLower(),
                UserName = model.Email.ToLower(),
                //EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(userToAdd, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await userManager.AddToRoleAsync(userToAdd, SD.PlayerRole);

            //return Ok("Your account has been created, you can now login.");

            try
            {
                if (await SendConfirmEmailAsync(userToAdd))
                {
                    return Ok(new JsonResult(
                            new { title = "Account Created", message = "Your account has been created, please confirm your email address" }
                            )
                        );
                }

                return BadRequest("Failed to send email. Please contact admin");
            }
            catch (Exception ex)
            {

                return BadRequest("Failed to send email. Please contact admin");
            }
            //return Ok(
            //    new JsonResult(
            //        new { title = "Account Created", message = "Your account has been created, you can now login" }
            //        )
            //    );

        }

        [HttpPost("register-with-third-party")]
        public async Task<ActionResult<UserDto>> RegisterWithThirdParty(RegisterWithExternal model)
        {
            if (model.Provider.Equals(SD.Facebook))
            {
                try
                {
                    if (!FacebookValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unable to register with facebook");
                    }
                }
                catch (Exception e)
                {

                    return Unauthorized();
                }
            }
            else if (model.Provider.Equals(SD.Google))
            {
                try
                {
                    if (!GoogleValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unable to register with google");
                    }
                }
                catch (Exception e)
                {
                    return Unauthorized();
                }
            }
            else
            {
                return BadRequest("Invalid provider");
            }

            var user = await userManager.FindByNameAsync(model.UserId);

            if (user != null)
            {
                return BadRequest(string.Format("You have an account already. Please login with your {0}", model.Provider));
            }

            var userToAdd = new User
            {
                FirstName = model.FirstName.ToLower(),
                LastName = model.LastName.ToLower(),
                UserName = model.UserId,
                Provider = model.Provider,
            };

            var result = await userManager.CreateAsync(userToAdd);

            if (!result.Succeeded) { return BadRequest(result.Errors); }
            await userManager.AddToRoleAsync(userToAdd, SD.PlayerRole);

            return await CreateApplicationUserDto(userToAdd);
        }

        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized("This email address has not been registered");
            }

            if (user.EmailConfirmed == true)
            {
                return BadRequest("You email was confirmed before. Please login to your account");
            }

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await userManager.ConfirmEmailAsync(user, decodedToken);

                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Email confirmed", message = "Your email address is confirmed. You can login now" }));
                }

                return BadRequest("Invalid token. Please try again.");
            }
            catch (Exception e)
            {
                return BadRequest("Invalid token. Please try again.");
            }
        }

        [HttpPost("resend-email-confirmation-link/{email}")]
        public async Task<IActionResult> ResetEmailConfirmationLink(string email)
        {

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid email");
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null) { return BadRequest("This email address has not been registered yet"); }

            if (user.EmailConfirmed == true) return BadRequest("Your email has been confirmed. Please login to your account");

            try
            {
                if (await SendConfirmEmailAsync(user))
                {
                    return Ok(
                        new JsonResult(
                            new { title = "Confirmation link sent", message = "Please confirm your email address" }
                            )
                        );
                }

                return BadRequest("Failed to send email. Please contact admin.");
            }
            catch (Exception e)
            {
                return BadRequest("Failed to send email. Please contact admin.");
            }
        }

        [HttpPost("forgot-username-or-password/{email}")]
        public async Task<IActionResult> ForgotUsernameOrPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid email");
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null) { return BadRequest("This email address has not been registered yet"); }

            if (user.EmailConfirmed == false) return BadRequest("Please confirm your email address first.");

            try
            {
                if (await SendForgotUsernameOrPassword(user))
                {
                    return Ok(new JsonResult(new { title = "Forgot username or password email sent", message = "Please check your email" }));
                }

                return BadRequest("Failed to send email. Please contact admin.");
            }
            catch (Exception e)
            {
                return BadRequest("Failed to send email. Please contact admin.");
            }
        }

        //put because updating
        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) { return BadRequest("This email address has not been registered yet"); }

            if (user.EmailConfirmed == false) return BadRequest("Please confirm your email address first");

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Password ret success", message = "Your password has been reset" }));
                }

                return BadRequest("Invalid token. Please try again.");
            }
            catch (Exception e)
            {

                return BadRequest("Invalid token. Please try again.");
            }
        }

        #region Private Helper Method
        private async Task<UserDto> CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = await jWTService.CreateJWT(user)
            };
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }

        private async Task<bool> SendConfirmEmailAsync(User user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var url = $"{configuration["JWT:ClientUrl"]}/{configuration["Email:ConfirmEmailPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                "<p>Please confirm your email address by clicking on the following link.</p>" +
                $"<p><a href=\"{url}\">Click Here</p>" +
                "<p>Thank you,</p>" +
                $"<br>{configuration["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto("Confirm your email", user.Email, body);

            return await emailService.SendEmailASync(emailSend);


        }

        private async Task<bool> SendForgotUsernameOrPassword(User user)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var url = $"{configuration["JWT:ClientUrl"]}/{configuration["Email:ResetPasswordPath"]}?token={token}&email={user.Email}";

            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                $"<p>Username: {user.UserName}</p>" +
                "<p>In order to reset your password, please click on the following link.</p>" +
                $"<p><a href=\"{url}\">Click Here</p>" +
                "<p>Thank you,</p>" +
                $"<br>{configuration["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto("Forgot username or password", user.Email, body);

            return await emailService.SendEmailASync(emailSend);
        }

        private async Task<bool> FacebookValidatedAsync(string accessToken, string userId)
        {
            var facebookKeys = configuration["Facebook:AppId"] + "|" + configuration["Facebook:AppSecret"];
            var fbResult = await _facebookHttpClient.GetFromJsonAsync<FacebookResultDto>($"debug_token?input_token={accessToken}&access_token={facebookKeys}");
            if (fbResult == null || fbResult.Data.Is_Valid == false || !fbResult.Data.user_id.Equals(userId))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task<bool> GoogleValidatedAsync(string accessToken, string userId)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(accessToken);
            if (!payload.Audience.Equals(configuration["Google:ClientId"]))
            {
                return false;
            }

            if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
            {
                return false;
            }

            if (payload.ExpirationTimeSeconds == null)
            {
                return false;
            }

            DateTime now = DateTime.Now.ToUniversalTime();
            DateTime expiration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
            if (now > expiration) { return false; }

            if (!payload.Subject.Equals(userId)) { return false; }

            return true;

        }
        #endregion
    }
}
