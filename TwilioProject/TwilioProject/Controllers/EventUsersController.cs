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
    public class EventUsersController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        // GET: EventUsers
        public ActionResult Index()
        {
            return View();
        }

        // GET: Host Controller
        public ActionResult IndexHost()
        {
            var user = User.Identity.GetUserId();
            var requiredData =
                (from x in db.Events
                 where x.HostID == user
                 select x).FirstOrDefault();
            var isActiveEvent = (requiredData != null) ? true : false;
            // if isactive = true
            // do event code turnary
            if (isActiveEvent)
            {
                var eventCode = (requiredData.IsHosted == true) ? requiredData.EventCode : null;
                if (eventCode != null)
                {
                    ViewBag.EventCode = eventCode;
                }
            }
            else
            {
                ViewBag.EventCode = "There is no current event";
            }
            return View();
        }
        public ActionResult QueueList()
        {
            return PartialView();
        }
        public ActionResult AttendeeIndex()
        {
            return View();
        }
        
        //
        // GET: Host Creat
        public ActionResult CreateHost()
        {
            return View();
        }
        //
        // POST: Host Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateHost(EventUsers model)
        {
            var user = User.Identity.GetUserId();
            var newID = new Guid().ToString();            
            var newHost = new EventUsers { PhoneNumber = model.PhoneNumber, AppUserId = user, UserID = user };
            db.EventUsers.Add(newHost);
            db.SaveChanges();
            return RedirectToAction("IndexHost");
        }


    }
}