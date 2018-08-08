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
<<<<<<< HEAD

        public ActionResult QueueList()
        {
            return PartialView();
=======
>>>>>>> b9b2e36ce2626ef13a02aee44e3a7af63ef8a086
        }
    }
}