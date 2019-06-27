using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMS.Models.Entities.Collections;
using GMS.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreaController : ControllerBase
    {
        private readonly AreaRepository repos = new AreaRepository();
        private readonly TreeRepository repos_Tree = new TreeRepository();
        private readonly TreeHistoryRepository treeHistoryRepository = new TreeHistoryRepository();

        // GET: api/Area
        [HttpGet]
        public async Task<List<Area>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/Area/id=value
        [HttpGet("id={id}")]
        public async Task<Area> Get(string id)
        {
            return await repos.Find(id);
        }

        // GET: api/Area/name=value
        [HttpGet("name={name}")]
        public async Task<List<Area>> GetByAttrName(string name)
        {
            return await repos.FindByAttr(name, "name");
        }

        // GET: api/Area/code=value
        [HttpGet("code={code}")]
        public async Task<List<Area>> GetByAttrCode(string code)
        {
            return await repos.FindByAttr(code, "code");
        }

        //GET: api/Area/trees/id=value
        [HttpGet("trees/id={id}")]
        public async Task<List<string>> GetAllTreeID(string id)
        {
            return await repos.GetAllTreeID(id);
        }


        // POST: api/Area
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]  Area area)
        {
            //check for valid object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for node's existence
            if (await repos.isExisted(area.Id))
            {
                return BadRequest("Phân vùng với id = " + area.Id + " đã tồn tại.");
            }

            //insert to database
            await repos.Create(area);
            return Ok();
        }

        // DELETE: api/Area/value
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            //check for node's existence
            if (!await repos.isExisted(id))
            {
                return NotFound("Phân vùng với id = " + id + " không tồn tại.");
            }

            var area = await repos.Find(id);
            foreach (string treeId in area.Trees)
            {
                await repos_Tree.Delete(treeId);
            }

            //delete out of database
            await repos.Delete(id);
            return Ok();
        }


        // DELETE: api/Area/delete
        [Authorize(Roles = "Admin")]
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteModel model)
        {
            var area = await repos.Find(model.PolygonId);
            if (area == null)
            {
                return BadRequest("Lỗi khi xóa phân vùng. Không thể tìm thấy phân vùng với id: " + model.PolygonId);
            }


            foreach (string TreeId in area.Trees)
            {
                var tree = await repos_Tree.Find(TreeId);
                if (tree == null)
                {
                    return BadRequest("Lỗi khi xóa phân vùng. Không thể tìm thấy cây với id: " + TreeId);
                }

                var history = new TreeHistory()
                {
                    Date = model.Date,
                    Name = "delete",
                    PolygonId = model.PolygonId,
                    TreeId = TreeId,
                    UserId = model.UserId,
                    Detail = "Phân vùng bị loại bỏ"
                };

                var deleteHistory = await treeHistoryRepository.Create(history);

                tree.History.Add(deleteHistory.Id);

                tree.Status = deleteHistory;

                if (!await repos_Tree.Update(tree))
                {
                    return BadRequest(new { tree = tree.Id, error = "Lỗi khi xóa phân vùng. Không thể cập nhật cây" });
                }
                
            }

            area.IsDeleted = true;
            if (!await repos.Update(area))
            {
                return BadRequest(new { area = area.Id, error = "Không thể xóa phân vùng" });
            }

            return Ok(area.Id);
        }


        // PUT: api/Area/
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Area updateArea)
        {
            //check for valid object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for node's existence
            if (!await repos.isExisted(updateArea.Id))
            {
                return BadRequest("Phân vùng với id = " + updateArea.Id + " không tồn tại.");
            }

          
            
          
            //update to database
            await repos.Update(updateArea);
            return Ok();
        }

        // PUT: api/Area/update
        [Authorize(Roles = "Admin")]
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateModel model)
        {
            //check for valid object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var area = await repos.Find(model.area.Id);
            //check for node's existence

            if (area == null)
            {
                return BadRequest("Phân vùng với id = " + model.area.Id + " không tồn tại.");
            }

            model.area.Trees = area.Trees;

            foreach (string TreeId in model.DeleteTrees)
            {
                var tree = await repos_Tree.Find(TreeId);
                if (tree == null)
                {
                    return BadRequest("Lỗi khi sửa phân vùng. Không thể tìm thấy cây với id: " + TreeId);
                }

                var history = new TreeHistory()
                {
                    Date = model.Date,
                    Name = "delete",
                    PolygonId = model.area.Id,
                    TreeId = TreeId,
                    UserId = model.UserId,
                    Detail = "Phân vùng bị chỉnh sửa"
                };

                var deleteHistory = await treeHistoryRepository.Create(history);

                tree.History.Add(deleteHistory.Id);

                tree.Status = deleteHistory;

                if (!await repos_Tree.Update(tree))
                {
                    return BadRequest(new { tree = tree.Id, error = "Lỗi khi sửa phân vùng. Không thể cập nhật cây" });
                }

                model.area.Trees.Remove(TreeId);
            }

            if (!area.ManagerId.Equals(model.area.ManagerId))
            {
                foreach (string treeId in model.area.Trees)
                {
                    var tree = await repos_Tree.Find(treeId);
                    if (tree == null)
                    {
                        return NotFound($"Không tìm thấy cây với id '{treeId}' trong phân vùng với id '{model.area.Id}'.");
                    }

                    tree.ManagerId = model.area.ManagerId;

                    if (!await repos_Tree.Update(tree))
                    {
                        return BadRequest(new { tree = tree.Id, area = model.area.Id, error = "Không thể thay đổi người quản lý của cây" });
                    }
                }
            }


            //update to database
            if (! await repos.Update(model.area))
            {
                return BadRequest(new { area = model.area.Id, error = "Không thể sửa phân vùng" });
            }

            return Ok(model.area.Id);
        }

        public class DeleteModel
        {
            public DateTime Date;

            public string PolygonId;

            public string UserId;
        }

        public class UpdateModel
        {
            public Area area;

            public DateTime Date;

            public string UserId;

            public List<string> DeleteTrees;
        }
    }

    
}
