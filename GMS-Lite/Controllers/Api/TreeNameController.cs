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
    public class TreeNameController : ControllerBase
    {
        private readonly TreeNameRepository repos = new TreeNameRepository();

        // GET: api/TreeName
        [HttpGet]
        public async Task<List<TreeName>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/TreeName/id=value
        [HttpGet("id={id}")]
        public async Task<TreeName> Get(string id)
        {
            return await repos.Find(id);
        }
        
        // GET: api/TreeName/name=value
        [HttpGet("name={name}")]
        public async Task<TreeName> GetByAttrName(string name)
        {
            return await repos.FindByName(name);
        }

        

        // POST: api/TreeName
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]  TreeName treename)
        {
            //check for valid object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for tree name's existence
            if (await repos.FindByName(treename.Name) != null)
            {
                return BadRequest("Loại cây với tên " + treename.Name + " đã có.");
            }
            

            //insert to database
            await repos.Create(treename);
            
            return Ok();
        }

        // DELETE: api/TreeName/value
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            //check for node's existence
            if (!await repos.isExisted(id))
            {
                return NotFound("Không thể tìm loại cây!");
            }
            //delete out of database
            await repos.Delete(id);
            
            return Ok();
        }

        // PUT: api/TreeName/
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] TreeName updateTreeName)
        {
            //check for valid object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for node's existence
            if (!await repos.isExisted(updateTreeName.Id))
            {
                return NotFound("Không thể tìm loại cây!");
            }
            
            //update to database
            await repos.Update(updateTreeName);
            return Ok();
        }
    }
}