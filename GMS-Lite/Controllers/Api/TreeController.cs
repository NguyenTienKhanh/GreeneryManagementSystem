using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMS.Models.Entities.Collections;
using GMS.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace GMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreeController : ControllerBase
    {
        private readonly TreeRepository repos = new TreeRepository();
        private readonly AreaRepository AreaRepos = new AreaRepository();
        private readonly TreeHistoryRepository HistoryRepos = new TreeHistoryRepository();

        // GET: api/Tree
        [HttpGet]
        public async Task<List<Tree>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/Tree/id=value
        [HttpGet("id={id}")]
        public async Task<Tree> Get(string id)
        {
            return await repos.Find(id);
        }

        // GET: api/Tree/name=value
        [HttpGet("name={name}")]
        public async Task<List<Tree>> GetbyAttrName(string name)
        {
            return await repos.Find(name, "name");
        }

        // GET: api/Tree/note=value
        [HttpGet("note={note}")]
        public async Task<List<Tree>> GetbyAttrStatus(string note)
        {
            return await repos.Find(note, "note");
        }

        // POST: api/Tree
        [Authorize(Roles = "Admin,Member")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]  Tree tree)
        {
            //check for valid tree object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for valid given info
            if (
                tree.Coord == null
                || tree.Name == null
                || tree.PolygonId == null
                || tree.ManagerId == null
                || tree.CreateUserId == null
                )
            {
                return BadRequest("Thông tin cây không hợp lệ !.");
            }

            var history = new TreeHistory()
            {
                Date = tree.RegisDate,
                Name = "create",
                PolygonId = tree.PolygonId,
                TreeId = "",
                UserId = tree.CreateUserId,
                Detail = ""
            };

            var createdHistory = await HistoryRepos.Create(history);

            tree.History = new List<string>
            {
                createdHistory.Id
            };

            tree.Status = createdHistory;

            //insert new tree to collection
            tree = await repos.Create(tree);

            createdHistory.TreeId = tree.Id;

            tree.Status = createdHistory;

            if (!await repos.Update(tree))
            {
                return BadRequest(new { tree = tree.Id, error = "cannot update status of tree" });
            }

            if (!await HistoryRepos.Update(createdHistory))
            {
                return BadRequest(new { history = createdHistory.Id, error = "cannot update createId of history" });
            }

            //add tree's reference to area treeId list
            if (!await AreaRepos.AddTreeId(tree.PolygonId, tree.Id))
            {
                return BadRequest(new { area = tree.PolygonId, error = "cannot add tree with id of " + tree.Id + " out of area" });
            }


            return Ok(tree.Id);
        }

        // DELETE: api/Tree/value
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            //check for valid id
            var objectId = new ObjectId();
            if (!ObjectId.TryParse(id, out objectId))
            {
                return BadRequest("Id không hợp lệ !.");
            }
            //check existence for tree
            if (!await repos.isExisted(id))
                return NotFound(new { id, error = "Không tồn tại cây với id = " + id + "." });
            var tree = await repos.Find(id);

            //delete tree out of collection
            if (await repos.Delete(id))
            {
                //remove tree's reference out of section's storage
                if (!await AreaRepos.DeleteTreeId(tree.PolygonId, tree.Id))
                {
                    return BadRequest(new { area = tree.PolygonId, error = "cannot remove tree with id of " + id + " out of area" });
                }
                return Ok();
            }
            else
                return BadRequest("Thất bại !.");
        }

        [Authorize(Roles = "Admin,Member")]
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteModel model)
        {
            var tree = await repos.Find(model.TreeId);
            if (tree == null)
            {
                return BadRequest("Không thể tìm thấy cây với id: " + model.TreeId);
            }

            var history = new TreeHistory()
            {
                Date = model.Date,
                Name = model.Name,
                PolygonId = model.PolygonId,
                TreeId = model.TreeId,
                UserId = model.UserId,
                Detail = model.Detail
            };

            var deleteHistory = await HistoryRepos.Create(history);

            tree.History.Add(deleteHistory.Id);

            tree.Status = deleteHistory;

            if (!await repos.Update(tree))
            {
                return BadRequest(new { tree = tree.Id, error = "Không thể cập nhật cây" });
            }

            var area = await AreaRepos.Find(tree.PolygonId);
            if (area == null)
            {
                return BadRequest("Không thể tìm thấy phân vùng với id: " + tree.PolygonId);
            }

            area.Trees.Remove(tree.Id);
            if (!await AreaRepos.Update(area))
            {
                return BadRequest(new { area = tree.PolygonId, error = "Không thể cập nhật phân vùng" });
            }

            return Ok(deleteHistory.Id);
        }

        // PUT: api/Tree/
        [Authorize(Roles = "Admin,Member")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateModel model)
        {
            //check for valid tree object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for valid given info
            if (
                model.newTree.Coord == null
                || model.newTree.Name == null
                || model.newTree.PolygonId == null
                || model.newTree.ManagerId == null
                )
            {
                return BadRequest("Thông tin cây không hợp lệ !.");
            }

            var newTree = model.newTree;
            //check for valid id
            var objectId = new ObjectId();
            if (!ObjectId.TryParse(newTree.Id, out objectId))
            {
                return BadRequest("Id không hợp lệ !.");
            }

            var OldTree = await repos.Find(newTree.Id);
            //check existence for tree
            if (OldTree == null)
                return NotFound(new { newTree.Id, error = "Không tồn tại cây với id = " + newTree.Id + "." });

            newTree.History = OldTree.History;

            var detail = new StringBuilder();
            if (OldTree.Height != newTree.Height)
            {
                detail.Append($"Thay đổi chiều cao từ {OldTree.Height} thành {newTree.Height}");
                detail.AppendLine();
            }
            if (OldTree.Width != newTree.Width)
            {
                detail.Append($"Thay đổi chiều rộng từ {OldTree.Width} thành {newTree.Width}");
                detail.AppendLine();
            }
            if (OldTree.Coord.Latitude != newTree.Coord.Latitude || OldTree.Coord.Longtitude != newTree.Coord.Longtitude)
            {
                detail.Append($"Tọa độ thay đổi từ [{OldTree.Coord.Latitude},{OldTree.Coord.Longtitude}] thành [{newTree.Coord.Latitude},{newTree.Coord.Longtitude}]");
                detail.AppendLine();
            }

            var history = new TreeHistory()
            {
                Date = model.Date,
                Detail = detail.ToString(),
                Name = "edit",
                PolygonId = newTree.PolygonId,
                TreeId = newTree.Id,
                UserId = model.UserId
            };

            var EditHistory = await HistoryRepos.Create(history);

            newTree.History.Add(EditHistory.Id);
            newTree.Status = EditHistory;

            //update tree
            if (await repos.Update(newTree))
            {
                return Ok();
            }
            else
                return BadRequest("Thất bại !.");
        }

        public class DeleteModel
        {
            public string TreeId;

            public string Detail;

            public DateTime Date;

            public string Name;

            public string PolygonId;

            public string UserId;
        }

        public class UpdateModel
        {
            public Tree newTree;

            public DateTime Date;

            public string UserId;
        }
    }

}
