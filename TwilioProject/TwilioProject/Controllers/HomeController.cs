using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TwilioProject.Models;

namespace TwilioProject.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public async Task<ActionResult> Index()
        {
            //var video = "6tgAJtvRP70";
            var video = db.Playlist.First();
            SmsController.currentVideo = video;
            Songs song = new Songs();
            song.EventID = db.EventUsers.Where(p => p.PhoneNumber == video.PhoneNumber).Single().EventID;
            song.SongLength = 4; //LOL
            song.YoutubeId = video.YoutubeID;
            ViewBag.Video = $"https://www.youtube.com/embed/{video.YoutubeID}?autoplay=1&enablejsapi=1";
            db.Songs.Add(song);
            db.Playlist.Remove(video);
            db.SaveChanges();
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Landing()
        {
            return View();
        }
        public ActionResult _PartialRegiter()
        {
            return RedirectToAction("_PartialRegister", "Account");
        }
    }
}