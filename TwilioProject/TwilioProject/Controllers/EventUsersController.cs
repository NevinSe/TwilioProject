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
                (from x in db.EventUsers
                 where x.AppUserId == user
                 select x).FirstOrDefault();
            var requiredEventData =
                (from x in db.Events
                 where x.HostID == user
                 select x).FirstOrDefault();
            var myEvent =
                (from x in db.Events
                 where x.IsHosted == true
                 select x).FirstOrDefault();

            var isActiveEvent = (requiredData != null) ? true : false;
            // if isactive = true
            // do event code turnary
            if (isActiveEvent)

            {
                var eventCode = (requiredEventData.IsHosted == true) ? requiredEventData.EventCode : null;
                if (eventCode != null)
                {
                    ViewBag.EventCode = myEvent.EventCode;
                }
            }
            else
            {
                ViewBag.EventCode = "There is no current event";
            }
            return View();
        }
        [ChildActionOnly]
        public ActionResult QueueList()
        {
            var x = db.Playlist.Select(y => y.SongOrderID).ToArray();
            Array.Sort(x);
            List<Playlist> queue = new List<Playlist>();
            int queueCount = (x.Count() < 5) ? x.Count() : 5;
            for(int i = 0; i < queueCount; i++)
            {
                var songOrderID = x[i];
                var newItem = db.Playlist.Where(y => y.SongOrderID == songOrderID).Select(y => y).FirstOrDefault();
                
                queue.Add(newItem);
            }
            ViewBag.Queue = queue;
            return PartialView();
        }
        [ChildActionOnly]
        public ActionResult PopularList()
        {
            
            var y = db.Songs.OrderByDescending(x => x.Likes).Take(5).Select(x => x).ToList();
            ViewBag.TopList = y;
            return PartialView();

        }
        public ActionResult AttendeeIndex()
        {
            return View();
        }
        public ActionResult SongSearchResults()
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
        //GET: AttendeeSongRequest
        public ActionResult _PartialAttendeeSongRequest()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //POST: AttendeeSongRequest
        public ActionResult _PartialAttendeeSongRequest(Songs song)
        {
            YoutubeSearch songSearch = new YoutubeSearch();
            var searchResults = songSearch.SearchByTitle(song.Title);
            List<string> songList = new List<string>();
            for (var i = 0; i < searchResults.Count; i++)
            {
                string result = searchResults[i][0];
                songList.Add(result);
            }
            ViewBag.SongList = songList;
            return View("SongSearchResults");
        }
        //
        // Get Banned Song
        [ChildActionOnly]
        public ActionResult ManageBannedSongs()
        {
            var x = db.Playlist.Select(y => y).ToArray();
            Array.Sort(x);
            Playlist newitem = x[0];
            ViewBag.CurrentSong = newitem;
            return PartialView();
        }
        // Post Banned Song
        [HttpPost]
        public ActionResult ManageBannedSOngs(Playlist toBan)
        {
            Songs newSong = new Songs();
            newSong.YoutubeId = toBan.YoutubeID;
            newSong.IsBanned = true;
            return RedirectToAction("IndexHost");
        }
    }
}
