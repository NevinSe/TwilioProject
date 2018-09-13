using System.Web.Mvc;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using System.Linq;
using System.Data;
using TwilioProject.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TwilioProject.Controllers
{
    public class SmsController : TwilioController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Regex regex = new Regex(@"\b[A-Z0-9a-z]{5}\b");
        private Regex songSelection = new Regex(@"^[1-5]$");
        private Regex banUser = new Regex(@"^ban [(]?\d{3}[)]?[-]?\s?\d{3}\s?[-]?\d{4}$");
        private Regex setVol = new Regex(@"^set volume \d?\d?\d[%]?$");
        private YoutubeSearch search = new YoutubeSearch();
        public static Songs currentVideo;
        public static Playlist whoPlayed;
        private List<string[]> videos = new List<string[]>();
        private bool isCompleted = false;
        public ActionResult YoutubeIndex()
        {
            VideoViewModel videoViewModel = new VideoViewModel();
            var video = db.Playlist.First();
            Songs song = new Songs();
            song.EventID = db.EventUsers.Where(p => p.PhoneNumber == video.PhoneNumber).Single().EventID;
            song.SongLength = 4;
            song.Title = video.Title;
            song.YoutubeId = video.YoutubeID;
            song.IsBanned = false;
            videoViewModel.youtubeId = $"https://www.youtube.com/embed/{video.YoutubeID}?autoplay=1&enablejsapi=1";
            SmsController.currentVideo = song;
            SmsController.whoPlayed = video;
            db.Songs.Add(song);
            db.Playlist.Remove(video);
            db.SaveChanges();
            return View(videoViewModel);
        }
        [HttpPost]
        public ActionResult Index()
        {
            var requestPhoneNumber = Request.Form["From"];
            var requestBody = Request.Form["Body"];
            var hPN = Phone.Parse(requestPhoneNumber);
            var hostPhoneNumber = db.Users.Where(p => p.PhoneNumber == hPN).SingleOrDefault();
            // Event Code
            if (regex.IsMatch(requestBody) && requestBody.ToLower() != "queue" && !db.EventUsers.Any(c => c.PhoneNumber.Contains(requestPhoneNumber)))
            {
                return EventCode(requestPhoneNumber, requestBody);
            }
            // Get Phone Number Of Current Song
            else if (requestBody.ToLower() == "who played this")
            {
                return SendMessage(whoPlayed.PhoneNumber);
            }
            // Ban User
            else if (banUser.IsMatch(requestBody.ToLower().Trim()) && Phone.Parse(requestPhoneNumber) == hPN)
            {
                var phoneNumber = Phone.Parse(requestBody);
                var bannedUser = db.EventUsers.Where(e => e.PhoneNumber == phoneNumber).SingleOrDefault();
                if (bannedUser != default(EventUsers))
                {
                    db.EventUsers.Where(p => p.PhoneNumber == bannedUser.PhoneNumber).Single().isBanned = true;
                    db.SaveChanges();
                    return SendMessage($"{bannedUser.PhoneNumber} Has been banned from adding songs to the queue.");
                }
                else
                {
                    return SendMessage("User not found.");
                }
            }
            // Ban Current Song
            else if (requestBody.ToLower() == "ban song" && Phone.Parse(requestPhoneNumber) == hPN)
            {
                db.Songs.Where(s => s.YoutubeId == currentVideo.YoutubeId).Single().IsBanned = true;
                db.SaveChanges();
                return SendMessage("This song has been banned");
                //SkipSong();
            }
            // Skip Song
            else if (requestBody.ToLower() == "skip")
            {
                SkipSong();
            }
            // Song Selection
            else if (songSelection.IsMatch(requestBody))
            {
                SelectSong(requestBody, Phone.Parse(requestPhoneNumber));
                return SendMessage("Your song has been added to the Queue");
            }
            // Help Host
            else if (requestBody.ToLower() == "commands" && hostPhoneNumber.Id == db.Events.First().HostID)
            {
                string hostHelpString = "Event Host Commands:\r\nBan User: 'ban (phone number)'\r\nBan Current Song: 'ban song'\r\n" +
                    "Skip Current Song: 'skip'\r\n" +
                    "Who Played Current Song: 'who played this'\r\n" +
                    "List Of The Song Queue: 'queue'\r\nList Of 'Hot' Songs: 'hot'\r\n" +
                    "Like A Song: 'like'\r\nDislike A Song: 'dislike'\r\nRequest A Song To Be Added: 'songtitle'";
                return SendMessage(hostHelpString);
            }
            // Help User
            else if (requestBody.ToLower() == "commands")
            {
                string userHelpString = "Event User Commands:\r\n" +
                    "Who Is Playing The Current Song: 'who played this'\r\n" +
                    "Get A List Of The Song Queue: 'queue'\r\n" +
                    "Get A List Of 'Hot' Songs: 'hot'\r\n" +
                    "Like A Song: 'like'\r\nDislike A Song: 'dislike'\r\n" +
                    "Request A Song To Be Added: 'songtitle'";
                return SendMessage(userHelpString);
            }
            // Queue
            else if (requestBody.ToLower() == "queue")
            {
                var videos = db.Playlist;
                var messageString = "";
                var counter = 1;
                foreach (var video in videos)
                {
                    messageString += $"{counter}.) {video.Title}\r\n";
                    counter++;
                }
                return SendMessage(messageString);
            }
            // Hot Songs
            else if (requestBody.ToLower() == "hot")
            {
                int numberOfHotSongs = 5;
                string messageString = "The Top Songs Are:\r\n";
                int counter = 1;
                var parsePhone = Phone.Parse(requestPhoneNumber);
                var currentSongs = db.Songs.OrderByDescending(o => o.Likes).Take(numberOfHotSongs).Select(p => p).ToList();
                foreach (Songs song in currentSongs)
                {
                    messageString += $"{counter}.) {song.Title}\r\n";
                    counter++;
                }
                return SendMessage(messageString);
            }
            // Like
            else if (requestBody.ToLower() == "like")
            {
                db.Songs.Where(s => s.YoutubeId == currentVideo.YoutubeId).First().Likes++;
                db.SaveChanges();
                return SendMessage("You have liked the current song.");
            }
            // Dislike
            else if (requestBody.ToLower() == "dislike")
            {
                db.Songs.Where(s => s.YoutubeId == currentVideo.YoutubeId).First().Dislikes++;
                db.SaveChanges();
                return SendMessage("You have disliked the current song.");
            }
            // Song Search
            else
            {
                Search(requestBody.ToLower().Trim());
                IdAndTitleToDB(videos, Phone.Parse(requestPhoneNumber));
                return SendMessage(VideosToMessage(videos));
            }
            return SendMessage("debug");
        }
        public void Search(string requestBody)
        {
            videos = search.SearchByTitle(requestBody);
            if (videos.Count != 0)
            {
                isCompleted = true;
            }
        }
        public ActionResult SkipSong()
        {
            return SendMessage("Not Yet Implemented.");
            //var controller = DependencyResolver.Current.GetService<HomeController>();
            //controller.ControllerContext = new ControllerContext(this.Request.RequestContext, controller);
            //ControllerActionInvoker controllerActionInvoker = new ControllerActionInvoker();
            ////HomeController homeController = new HomeController();
            ////controllerContext.Controller.ControllerContext = homeController.ControllerContext;
            //controllerActionInvoker.InvokeAction(controller.ControllerContext, "IndexTry");

        }
        public string VideosToMessage(List<string[]> videos)
        {
            string titles = "";
            for (int i = 0; i < 5; i++)
            {
                titles += $"{i + 1}.) {videos[i][0]}\r\n";
            }
            return titles;
        }
        public void IdAndTitleToDB(List<string[]> videos, string phoneNumber)
        {
            var person = db.EventUsers.Where(e => e.PhoneNumber == phoneNumber).Single();
            person.Id1 = videos[0][1];
            person.Id2 = videos[1][1];
            person.Id3 = videos[2][1];
            person.Id4 = videos[3][1];
            person.Id5 = videos[4][1];
            person.Title1 = videos[0][0];
            person.Title2 = videos[1][0];
            person.Title3 = videos[2][0];
            person.Title4 = videos[3][0];
            person.Title5 = videos[4][0];
            db.SaveChanges();
        }
        public void SelectSong(string songNumber, string phoneNumber)
        {
            var person = db.EventUsers.Where(e => e.PhoneNumber == phoneNumber).Single();
            Playlist playlist = new Playlist();
            switch (songNumber)
            {
                case "1":
                    playlist.YoutubeID = person.Id1;
                    playlist.Title = person.Title1;
                    playlist.PhoneNumber = person.PhoneNumber;
                    db.Playlist.Add(playlist);
                    break;
                case "2":
                    playlist.YoutubeID = person.Id2;
                    playlist.Title = person.Title2;
                    playlist.PhoneNumber = person.PhoneNumber;
                    db.Playlist.Add(playlist);
                    break;
                case "3":
                    playlist.YoutubeID = person.Id3;
                    playlist.Title = person.Title3;
                    playlist.PhoneNumber = person.PhoneNumber;
                    db.Playlist.Add(playlist);
                    break;
                case "4":
                    playlist.YoutubeID = person.Id4;
                    playlist.Title = person.Title4;
                    playlist.PhoneNumber = person.PhoneNumber;
                    db.Playlist.Add(playlist);
                    break;
                case "5":
                    playlist.YoutubeID = person.Id5;
                    playlist.Title = person.Title5;
                    playlist.PhoneNumber = person.PhoneNumber;
                    db.Playlist.Add(playlist);
                    break;
                default:
                    break;
            }
            db.SaveChanges();
        }
        public ActionResult EventCode(string number, string message)
        {
            string userPhone = Phone.Parse(number);
            // Find User by Number
            var user = db.EventUsers.Where(u => u.PhoneNumber == userPhone).SingleOrDefault();
            // New User
            if (user == default(EventUsers) && regex.IsMatch(message))
            {
                var myEvent = db.Events.Where(e => e.IsHosted == true && e.EventCode == message.ToLower()).SingleOrDefault();

                // Event Not Hosted or Wrong Code
                if (myEvent == default(Events))
                {
                    return SendMessage("Sorry, The Event You're Looking For Is Either Not Currently Hosted Or The Event Code Entered Is Incorrect.");
                }

                // Event Hosted and Code is Valid, New User
                else
                {
                    EventUsers newUser = new EventUsers
                    {
                        PhoneNumber = userPhone,
                        EventID = myEvent.EventID
                    };
                    newUser.isBanned = false;
                    db.EventUsers.Add(newUser);
                    db.SaveChanges();
                    return SendMessage($"You have been added to the event, {myEvent.EventName}.\r\n" +
                        $"Text 'commands' to receive a list of helpful commands.");
                }
            }

            // Returning User
            else if (user != default(EventUsers) && regex.IsMatch(message))
            {
                var myEvent = db.Events.Where(e => e.IsHosted == true && e.EventCode == message.ToLower()).SingleOrDefault();

                // EventHosted or Wrong Code
                if (myEvent == default(Events))
                {
                    return SendMessage("Sorry, The Event You're Looking For Is Either Not Currently Hosted Or The Event Code Entered Is Incorrect.");
                }

                // Event Hosted and Code Valid, Return User
                else
                {

                    user.EventID = myEvent.EventID;
                    db.SaveChanges();
                    return SendMessage($"You have been added to the event, {myEvent.EventName}." +
                        $"Text 'commands' to receive a list of helpful commands.");
                }
            }
            else
            {
                return SendMessage("Invalid Input. Please try again, or Type 'help' for more information.");
            }
        }
        public ActionResult SendMessage(string message)
        {
            MessagingResponse Message = new MessagingResponse();
            Message.Message(message);
            return TwiML(Message);
        }
    }
}