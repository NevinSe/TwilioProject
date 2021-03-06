﻿using System;
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
        public static bool SkipSong = false;
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            VideoViewModel videoViewModel = new VideoViewModel();
            var video = db.Playlist.First();
            Songs song = new Songs();
            song.EventID = db.EventUsers.Where(p => p.PhoneNumber == video.PhoneNumber).First().EventID;
            song.SongLength = 4;
            song.Title = video.Title;
            song.YoutubeId = video.YoutubeID;
            song.IsBanned = false;
            song.Likes = 0;
            song.Dislikes = 0;
            videoViewModel.youtubeId = $"https://www.youtube.com/embed/{video.YoutubeID}?autoplay=1&enablejsapi=1";
            ViewBag.Video = $"https://www.youtube.com/embed/{video.YoutubeID}?autoplay=1&enablejsapi=1";
            ViewBag.Skip = SkipSong;
            SmsController.currentVideo = song;
            SmsController.whoPlayed = video;
            db.Songs.Add(song);
            db.Playlist.Remove(video);
            db.SaveChanges();
            return View(videoViewModel);
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
        public ActionResult _PartialRegister()
        {
            return RedirectToAction("_PartialRegister", "Account");
        }
        public ActionResult HomeControllerSkipSong()
        {
            return RedirectToAction("Index");
        }
    }
}