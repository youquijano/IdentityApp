﻿using Api.DTOs.Admin;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdminController(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpGet("get-members")]
        public async Task<ActionResult<IEnumerable<MemberViewDto>>> GetMembers()
        {
            var members = await userManager.Users
                .Where(x => x.UserName != SD.AdminUserName)
                //this is a projection
                .Select(member => new MemberViewDto
                {
                    Id = member.Id,
                    UserName = member.UserName,
                    LastName = member.LastName,
                    DateCreated = member.DateCreated,
                    IsLocked = userManager.IsLockedOutAsync(member).GetAwaiter().GetResult(),
                    Roles = userManager.GetRolesAsync(member).GetAwaiter().GetResult()
                }).ToListAsync();

            return Ok(members);
        }

        [HttpGet("get-member/{id}")]
        public async Task<ActionResult<MemberAddEditDto>> GetMember(string id)
        {
            var member = await userManager.Users
                .Where(x => x.UserName != SD.AdminUserName && x.Id == id)
                .Select(m => new MemberAddEditDto
                {
                    Id = m.Id,
                    UserName = m.UserName,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Roles = string.Join(",", userManager.GetRolesAsync(m).GetAwaiter().GetResult())
                }).FirstOrDefaultAsync();

            return Ok(member);
        }

        [HttpPost("add-edit-member")]
        public async Task<IActionResult> AddEditMember(MemberAddEditDto model)
        {
            User user;
            if (string.IsNullOrEmpty(model.Id))
            {
                //adding a new member

                if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 6)
                {
                    ModelState.AddModelError("errors", "Password must be at least 6 characters");
                    return BadRequest(ModelState);
                }

                user = new User
                {
                    FirstName = model.FirstName.ToLower(),
                    LastName = model.LastName.ToLower(),
                    UserName = model.UserName.ToLower(),
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded) { return BadRequest(result.Errors); }



            }
            else
            {
                //editing an existing member

                if (!string.IsNullOrEmpty(model.Password))
                {
                    if (model.Password.Length < 6)
                    {
                        ModelState.AddModelError("errors", "Password must be at least 6 characters");
                        return BadRequest(ModelState);
                    }
                }

                if (IsAdminUserId(model.Id))
                {
                    return BadRequest(SD.SuperAdminChangeNotAllowed);
                }

                user = await userManager.FindByIdAsync(model.Id);

                if (user == null) { return NotFound(); }

                user.FirstName = model.FirstName.ToLower();
                user.LastName = model.LastName.ToLower();
                user.UserName = model.UserName.ToLower();

                if (!string.IsNullOrEmpty(model.Password))
                {
                    await userManager.RemovePasswordAsync(user);
                    await userManager.AddPasswordAsync(user, model.Password);
                }
            }

            var userRoles = await userManager.GetRolesAsync(user);

            //remove users existing roles
            await userManager.RemoveFromRolesAsync(user, userRoles);

            foreach (var role in model.Roles.Split(",").ToArray())
            {
                var roleToAdd = await roleManager.Roles.FirstOrDefaultAsync(r => r.Name == role);
                if (roleToAdd != null)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }

            if (string.IsNullOrEmpty(model.Id))
            {


                return Ok(new JsonResult(
                                new { title = "Member Created", message = $"{model.UserName} has been created" }
                                )
                            );
            }
            else
            {
                return Ok(new JsonResult(
                            new { title = "Member Edited", message = $"{model.UserName} has been updated" }
                            )
                        );
            }
        }

        //put since we are modifying
        //IActionResult when we are not expecting anything
        [HttpPut("lock-member/{id}")]
        public async Task<IActionResult> LockMember(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null) { return NotFound(); }

            if (IsAdminUserId(id))
            {
                return BadRequest(SD.SuperAdminChangeNotAllowed);
            }

            await userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddDays(5));

            return NoContent();
        }

        [HttpPut("unlock-member/{id}")]
        public async Task<IActionResult> UnlockMember(string id)
        {

            var user = await userManager.FindByIdAsync(id);

            if (user == null) { return NotFound(); }

            if (IsAdminUserId(id))
            {
                return BadRequest(SD.SuperAdminChangeNotAllowed);
            }

            await userManager.SetLockoutEndDateAsync(user, null);

            return NoContent();
        }

        [HttpDelete("delete-member/{id}")]
        public async Task<IActionResult> DeleteMember(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null) { return NotFound(); }

            if (IsAdminUserId(id))
            {
                return BadRequest(SD.SuperAdminChangeNotAllowed);
            }

            await userManager.DeleteAsync(user);

            return NoContent();
        }

        [HttpGet("get-application-roles")]
        public async Task<ActionResult<string[]>> GetApplicationRoles()
        {
            return Ok(await roleManager.Roles.Select(x => x.Name).ToListAsync());
        }



        #region Private Methods
        private bool IsAdminUserId(string id)
        {
            return userManager.FindByIdAsync(id).GetAwaiter().GetResult().UserName.Equals(SD.AdminUserName);
        }
        #endregion

    }
}
