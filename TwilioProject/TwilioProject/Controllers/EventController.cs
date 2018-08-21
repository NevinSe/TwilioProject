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

            //var requiredData =
            //    (from x in db.EventUsers
            //     where x.UserID == user
            //     select x).FirstOrDefault();

            var createdEvent = new Events { EventName = newEvent.EventName, HostID = user, EventCode = newEvent.EventCode, IsHosted = true };
            db.Events.Add(createdEvent);
            db.SaveChanges();
            return RedirectToAction("IndexHost", "EventUsers");
        }

        //
        // GET: /Event/_PartialManageEvent
        public ActionResult _PartialManageEvent()
        {
            var user = User.Identity.GetUserId();
            var myEvent = db.EventUsers.Find(user).EventID;
            var currentEvent = db.Events.Find(myEvent);
            return PartialView(currentEvent);
        }
        //
        // POST: /Event/_PartialManageEvent
        [HttpPost]
        public ActionResult _PartialManageEvent(Events model)
        {
            var user = User.Identity.GetUserId();
            var myEvent = db.EventUsers.Find(user).EventID;
            var currentEvent =
                (from x in db.Events
                 where x.HostID == user
                 select x).FirstOrDefault();
            currentEvent.EventName = (model.EventName != null) ? model.EventName : currentEvent.EventName;
            currentEvent.EventCode = (model.EventCode != null) ? model.EventCode : currentEvent.EventCode;
            currentEvent.IsHosted = model.IsHosted;
            db.Entry(currentEvent).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("IndexHost", "EventUsers");
        }
    }
}


