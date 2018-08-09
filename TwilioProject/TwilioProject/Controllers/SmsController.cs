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
        [HttpPost]
        public ActionResult Index()
        {
            var requestPhoneNumber = Request.Form["From"];
            var requestBody = Request.Form["Body"];
            var hPN = Phone.Parse(requestPhoneNumber);
            var hostPhoneNumber = db.Users.Where(p => p.PhoneNumber == hPN).SingleOrDefault();
            // Event Code
            if (regex.IsMatch(requestBody) && !db.EventUsers.Any(c => c.PhoneNumber.Contains(requestPhoneNumber)))
            {
                return EventCode(requestPhoneNumber, requestBody);
            }
            // Get Phone Number Of Current Song
            else if (requestBody.ToLower() == "who played this")
            {
                return SendMessage(whoPlayed.PhoneNumber);
            }
            // Ban User
            else if (banUser.IsMatch(requestBody.ToLower())/* && Phone.Parse(requestPhoneNumber) == hostPhoneNumber*/)
            {
                var tempNumber = "";
                foreach (char character in requestBody)
                {
                    try
                    {
                        tempNumber += int.Parse(character.ToString()).ToString();
                    }
                    catch (System.Exception)
                    {
                        return SendMessage("meh");
                    }
                }
                var phoneNumber = Phone.Parse(tempNumber);
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
            else if (requestBody.ToLower() == "ban song" /*&& Phone.Parse(requestPhoneNumber) == hostPhoneNumber*/)
            {
                var currentSong = db.Playlist.First();
                db.Songs.Where(s => s.YoutubeId == currentSong.YoutubeID).Single().IsBanned = true;
                db.SaveChanges();
                SkipSong();
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
            }
            // Help Host
            else if (requestBody.ToLower() == "help12" && hostPhoneNumber != default(ApplicationUser))
            {
                string hostHelpString = "Event Host Commands:\r\nBan User: 'ban (phone number)'\r\nBan Current Song: 'ban song'\r\n" +
                    "Skip Current Song: 'skip'\r\n" +
                    "Who Played Current Song: 'who played this'\r\n" +
                    "List Of The Song Queue: 'queue'\r\nList Of 'Hot' Songs: 'hot'\r\n" +
                    "Like A Song: 'like'\r\nDislike A Song: 'song'\r\nRequest A Song To Be Added: 'songtitle'";
                return SendMessage(hostHelpString);
            }
            // Help User
            else if (requestBody.ToLower() == "help12" /*&& Phone.Parse(requestPhoneNumber) != hostPhoneNumber*/)
            {
                string userHelpString = "Event User Commands:\r\n" +
                    "Who Is Playing The Current Song: 'who played this'\r\n" +
                    "Get A List Of The Song Queue: 'queue'\r\n" +
                    "Get A List Of 'Hot' Songs: 'hot'\r\n" +
                    "Like A Song: 'like'\r\nDislike A Song: 'song'\r\n" +
                    "Request A Song To Be Added: 'songtitle'";
                return SendMessage(userHelpString);
            }
            // Queue
            else if (requestBody.ToLower() == "que")
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
                List<string> topSongTitles = new List<string>();
                int counter = 1;
                for (int i = 0; i < numberOfHotSongs; i++)
                {
                    var currentSong = db.Songs.Where(s => s.EventID == (db.EventUsers.Where(e => e.PhoneNumber == Phone.Parse(requestPhoneNumber)).Single().EventID)).OrderBy(o => o.Likes).ElementAtOrDefault(i);
                    if (currentSong == default(Songs))
                    {
                        topSongTitles.Add("");
                    }
                    else
                    {
                        topSongTitles.Add(currentSong.Title);
                    }
                }
                foreach (string songTitle in topSongTitles)
                {
                    messageString += $"{counter}.) {songTitle}\r\n";
                    counter++;
                }
                return SendMessage(messageString);
            }
            // Like
            else if (requestBody.ToLower() == "like")
            {
                var currentSong = db.Songs.Where(s => s.YoutubeId == currentVideo.YoutubeId).First();
                currentSong.Likes++;
                db.SaveChanges();
            }
            // Dislike
            else if (requestBody.ToLower() == "dislike")
            {
                var currentSong = db.Songs.Where(s => s.YoutubeId == currentVideo.YoutubeId).First();
                currentSong.Dislikes++;
                db.SaveChanges();
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
        public void SkipSong()
        {
            var controller = DependencyResolver.Current.GetService<HomeController>();
            controller.ControllerContext = new ControllerContext(this.Request.RequestContext, controller);
            ControllerActionInvoker controllerActionInvoker = new ControllerActionInvoker();
            //HomeController homeController = new HomeController();
            //controllerContext.Controller.ControllerContext = homeController.ControllerContext;
            controllerActionInvoker.InvokeAction(controller.ControllerContext, "Index");

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
            SendMessage("Your song has been added to the queue.");
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
                    db.EventUsers.Add(newUser);
                    db.SaveChanges();
                    return SendMessage($"You have been added to the event, {myEvent.EventName}.");
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
                    return SendMessage($"You have been added to the event, {myEvent.EventName}.");
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