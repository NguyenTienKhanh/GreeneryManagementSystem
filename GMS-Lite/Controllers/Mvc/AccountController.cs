using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMS.Models.Entities.Collections;
using GMS.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AreaRepository areaRepository;
        private readonly TreeRepository treeRepository;

        public AccountController(UserManager<IdentityUser> userManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _logger = logger;
            this.areaRepository = new AreaRepository();
            this.treeRepository = new TreeRepository();
        }

        [HttpPost]
        public async Task<JsonResult> UserAlreadyExistsAsync([Bind(Prefix = "Input.Username")]string Username)
        {
            var result =
                await _userManager.FindByNameAsync(Username);
            return Json(result == null);
        }

        [HttpGet("/Account/all")]
        public async Task<List<IdentityUser>> GetAll()
        {
            var result =
                await _userManager.Users.ToListAsync();
            return result;
        }

        [HttpGet("/Account/{id}")]
        public async Task<JsonResult> GetUserName(string id)
        {
            var result =
                await _userManager.FindByIdAsync(id);
            return Json(new { result.UserName, result.Email, result.PhoneNumber });
        }

        [HttpGet("/Account/current")]
        public async Task<JsonResult> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return Json(user?.Id);
        }

        //Account/{id}
        [Authorize(Roles = "Admin")]
        [HttpPost("/Account/Delete")]
        public async Task<IActionResult> DeleteUserById([FromBody] DeleteModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var deleteUser = await _userManager.FindByIdAsync(model.DeleteId);
            var replaceUser = await _userManager.FindByIdAsync(model.ReplaceId);
            if (deleteUser == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{model.DeleteId}'.");
            }

            if (replaceUser == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{model.ReplaceId}'.");
            }

            var areas = await areaRepository.FindAll();
            foreach (var area in areas)
            {
                if (area.ManagerId.Equals(deleteUser.Id))
                {
                    foreach (string treeId in area.Trees)
                    {
                        var tree = await treeRepository.Find(treeId);
                        if (tree == null)
                        {
                            return NotFound($"Không tìm thấy cây với id '{treeId}' trong phân vùng với id '{area.Id}'.");
                        }

                        tree.ManagerId = replaceUser.Id;

                        if (!await treeRepository.Update(tree))
                        {
                            return BadRequest(new { tree = tree.Id, area = area.Id, error = "Không thể thay đổi người quản lý của cây" });
                        }
                    }

                    area.ManagerId = replaceUser.Id;

                    if (!await areaRepository.Update(area))
                    {
                        return BadRequest(new { area = area.Id, error = "Không thể thay đổi người quản lý của phân vùng" });
                    }
                }
            }
            
            var result = await _userManager.DeleteAsync(deleteUser);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Xảy ra lỗi khi xóa người dùng với ID '{deleteUser.Id}'.");
            }


            _logger.LogInformation($"Người dùng với id='{deleteUser.Id}' đã bị xóa bởi người quản trị.");

            return Ok();
        }

        public class DeleteModel
        {
            public string DeleteId;
            public string ReplaceId;
        }
    }
}