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
            return View();
        }
        public ActionResult AttendeeIndex()
        {
            return View();
        }
    }
}