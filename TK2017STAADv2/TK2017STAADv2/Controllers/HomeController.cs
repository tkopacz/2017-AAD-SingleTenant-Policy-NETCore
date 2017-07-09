using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TK2017STAADv2.Models;
using System.Security.Claims;

namespace TK2017STAADv2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var claims = ((ClaimsIdentity)User.Identity).Claims;
            return View((object)claims);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /*Policies*/
        [Authorize(Policy = "OKPolicy")]
        public IActionResult DemoOKPolicy()
        {
            ViewData["Message"] = "OK - AdminPolicy";
            return View("Demo");
        }

        [Authorize(Policy = "GroupPolicyByGuid")]
        public IActionResult DemoAdminPolicyByGuid()
        {
            ViewData["Message"] = "Demo - GroupPolicyByGuid";
            return View("Demo");
        }


    }
}
