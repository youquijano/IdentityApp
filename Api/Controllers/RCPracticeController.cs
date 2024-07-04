using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RCPracticeController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("public");
        }

        #region Roles
        [HttpGet("admin-role")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminRole()
        {
            return Ok("Admin Role");
        }

        [HttpGet("manager-role")]
        [Authorize(Roles = "Manager")]
        public IActionResult ManagerRole()
        {
            return Ok("Manager Role");
        }

        [HttpGet("player-role")]
        [Authorize(Roles = "Player")]
        public IActionResult PlayerRole()
        {
            return Ok("Player Role");
        }

        [HttpGet("admin-or-manager-role")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult AdminOrManagerRole()
        {
            return Ok("Admin or Manager Role");
        }

        [HttpGet("admin-or-player-role")]
        [Authorize(Roles = "Admin,Player")]
        public IActionResult AdminOrPlayerRole()
        {
            return Ok("Admin or Player Role");
        }
        #endregion

        #region Policy
        [HttpGet("admin-policy")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult AdminPolicy()
        {
            return Ok("Admin Policy");
        }

        [HttpGet("manager-policy")]
        [Authorize(Policy = "ManagerPolicy")]
        public IActionResult ManagerPolicy()
        {
            return Ok("Manager Policy");
        }

        [HttpGet("player-policy")]
        [Authorize(Policy = "PlayerPolicy")]
        public IActionResult PlayerPolicy()
        {
            return Ok("Player Policy");
        }

        [HttpGet("admin-or-manager-policy")]
        [Authorize(Policy = "AdminOrManagerPolicy")]
        public IActionResult AdminOrManagerPolicy()
        {
            return Ok("Admin or Manager Policy");
        }

        [HttpGet("admin-and-manager-policy")]
        [Authorize(Policy = "AdminAndManagerPolicy")]
        public IActionResult AdminAndManagerPolicy()
        {
            return Ok("Admin and Manager Policy");
        }

        [HttpGet("all-role-policy")]
        [Authorize(Policy = "AllRolePolicy")]
        public IActionResult AllRolePolicy()
        {
            return Ok("All Role Policy");
        }
        #endregion

        #region Claim Policy
        [HttpGet("admin-email-policy")]
        [Authorize(Policy = "AdminEmailPolicy")]
        public IActionResult AdminEmailPolicy()
        {
            return Ok("Admin Email Policy");
        }

        [HttpGet("bi-surname-policy")]
        [Authorize(Policy = "BiSurnamePolicy")]
        public IActionResult BiSurnamePolicy()
        {
            return Ok("bi surname Policy");
        }

        [HttpGet("manager-email-and-wilson-surname-policy")]
        [Authorize(Policy = "ManagerEmailAndWilsonSurnamePolicy")]
        public IActionResult ManagerEmailAdnWilsonSurnamePolicy()
        {
            return Ok("Manager Email And Wilson Surname Policy");
        }

        [HttpGet("vip-policy")]
        [Authorize(Policy = "VIPPolicy")]
        public IActionResult VIPPolicy()
        {
            return Ok("VIP Policy");
        }
        #endregion
    }
}
