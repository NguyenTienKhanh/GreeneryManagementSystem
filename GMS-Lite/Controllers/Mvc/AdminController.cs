using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMS.Controllers
{
    public class AdminController : Controller
    {
        // GET: Request
        [Authorize(Roles ="Admin")]
        public ActionResult Index()
        {
            return View();
        }

        // GET: Request/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Test()
        {
            return View();
        }
    }
}