using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMS.Models.Entities.Collections;
using GMS.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreeHistoryController : ControllerBase
    {
        private readonly TreeHistoryRepository repos = new TreeHistoryRepository();
        private readonly List<string> types = new List<string>() { "create", "delete", "water", "pesticide", "fertilize", "edit", "pruning" };
        private readonly TreeRepository treeRepos = new TreeRepository();

        // GET: api/TreeHistory
        [HttpGet]
        public async Task<List<TreeHistory>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/TreeHistory/id=value
        [HttpGet("id={id}")]
        public async Task<TreeHistory> Get(string id)
        {
            return await repos.Find(id);
        }

        // GET: api/TreeHistory/option={option}/name={name}
        [HttpGet("option={option}/name={name}")]
        public async Task<List<TreeHistory>> GetByAttrName(string option, string name)
        {
            return await repos.FindByAttr(option, name);
        }

        // GET: api/TreeHistory/from={from}/to={to}
        [HttpGet("from={from}/to={to}")]
        public async Task<List<TreeHistory>> GetByRangeDate(string from, string to)
        {
            if (DateTime.TryParse(from, out DateTime From) && DateTime.TryParse(to, out DateTime To))
            {
                return await repos.FindByRangeDate(From, To);
            }
            return null;
        }

        // POST: api/TreeHistory/RangeDateStatus
        [HttpPost("RangeDateStatus")]
        public async Task<List<TreeHistory>> GetByRangeDateWithStatus(RangeModel model)
        {
            if (!ModelState.IsValid)
            {
                return null;
            }

            if (DateTime.TryParse(model.from, out DateTime From) && DateTime.TryParse(model.to, out DateTime To) && model.status != "")
            {
                return await repos.FindByRangeDateWithStatus(From, To, model.status);
            }
            return null;
        }

        // GET: api/TreeHistory/date={date}
        [HttpGet("date={date}")]
        public async Task<List<TreeHistory>> GetByDate(string date)
        {
            if (DateTime.TryParse(date, out DateTime Date))
            {
                return await repos.FindByDate(Date);
            }
            return null;
        }


        // POST: api/TreeHistory
        [Authorize(Roles = "Admin,Member")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]  TreeHistory history)
        {
            //check for valid object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!types.Contains(history.Name))
            {
                return BadRequest("Loại lịch sử không hợp lệ: " + history.Name);
            }

            //insert to database
            var result = await repos.Create(history);

            var tree = await treeRepos.Find(history.TreeId);

            if (tree == null)
            {
                return BadRequest("Không thể tìm thấy cây với id: " + history.TreeId);
            }

            tree.History.Add(result.Id);
            tree.Status = result;

            if (! await treeRepos.Update(tree))
            {
                return BadRequest("Không thể cập nhật cây với id: " + tree.Id);
            }

            return Ok(result.Id);
        }

        // DELETE: api/TreeHistory/value
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await repos.Delete(id))
            {
                return NotFound("Lịch sử với id = " + id + " không tồn tại.");
            }

            return Ok();
        }



        // PUT: api/TreeHistory/
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] TreeHistory updateHistory)
        {
            //check for valid object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for node's existence
            if (!await repos.isExisted(updateHistory.Id))
            {
                return BadRequest("Lịch sử với id = " + updateHistory.Id + " không tồn tại.");
            }

            //update to database
            await repos.Update(updateHistory);
            return Ok();
        }

        public class RangeModel
        {
            public string from;
            public string to;
            public string status;
        }
    }

  
}
