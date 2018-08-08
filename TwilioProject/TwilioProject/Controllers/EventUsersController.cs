using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TwilioProject.Controllers
{
    public class EventUsersController : Controller
    {
        // GET: EventUsers
        public ActionResult Index()
        {
            return View();
        }

        // GET: Host Controller
        public ActionResult IndexHost()
        {
            string[] testString = new string[] { "Option 1", "Option 2" };
            ViewBag.sudo = new SelectList(testString);
            return View();
        }

        public ActionResult QueueList()
        {
            return PartialView();
        }
    }
}