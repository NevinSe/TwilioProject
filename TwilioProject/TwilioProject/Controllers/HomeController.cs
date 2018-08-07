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

            var video = db.Playlist.First();
            ViewBag.Video = $"https://www.youtube.com/embed/{video.YoutubeID}?enablejsapi=1";
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
    }
}