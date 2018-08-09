using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TwilioProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace TwilioProject.Controllers
{
    public class EventController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        // GET: Event
        public ActionResult Index()
        {
            return View();
        }

        // GET: Create Event
        public ActionResult CreateEvent()
        {
            return PartialView();
        }

        // Post: Create Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEvent(Events newEvent)
        {
            var user = User.Identity.GetUserId();

            var requiredData =
                (from x in db.EventUsers
                 where x.UserID == user
                 select x).FirstOrDefault();

            var createdEvent = new Events { EventName = newEvent.EventName, HostID = user, EventCode = newEvent.EventCode, IsHosted = true };
            db.Events.Add(createdEvent);
            db.SaveChanges();
            return RedirectToAction("IndexHost", "EventUsers");
        }
    }
}