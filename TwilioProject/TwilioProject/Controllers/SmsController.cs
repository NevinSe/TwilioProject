using System.Web.Mvc;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using System.Linq;
using System.Data;
using TwilioProject.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace TwilioProject.Controllers
{
    public class SmsController : TwilioController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Regex regex = new Regex(@"[A-Z0-9a-z]{5}");
        private Regex songSelection = new Regex(@"[1-5]");
        private Regex banUser = new Regex(@"ban [(]?\d{3}[)]?[-]?\s?\d{3}\s?[-]?\d{4}");
        private YoutubeSearch search = new YoutubeSearch();
        private ApplicationUser applicationUser = new ApplicationUser();

        [HttpPost]
        public async void Index()
        {
            var requestPhoneNumber = Request.Form["From"];
            var requestBody = Request.Form["Body"];
            // Event Code
            if (regex.IsMatch(requestBody))
            {
                EventCode(requestPhoneNumber, requestBody);
            }
            // Ban User
            else if(banUser.IsMatch(requestBody.ToLower()) && Phone.Parse(requestPhoneNumber) == db.EventUsers.Where(e => e.PhoneNumber == Phone.Parse(requestPhoneNumber) && e.UserID == db.Events.Where(ev => ev.HostID == e.UserID).Single().HostID && db.Events.Where(p => p.HostID == e.UserID).Single().IsHosted).Single().PhoneNumber)
            {
                db.EventUsers.Where(p => p.PhoneNumber == banUser.ToString()).Single().isBanned = true;
                db.SaveChanges();
            }
            // Ban Current Song
            else if(requestBody.ToLower() == "ban song" && Phone.Parse(requestPhoneNumber) == db.EventUsers.Where(e => e.PhoneNumber == Phone.Parse(requestPhoneNumber) && e.UserID == db.Events.Where(ev => ev.HostID == e.UserID).Single().HostID && db.Events.Where(p => p.HostID == e.UserID).Single().IsHosted).Single().PhoneNumber)
            {
                var currentSong = db.Playlist.First();
                db.Songs.Where(s => s.YoutubeId == currentSong.YoutubeID).Single().IsBanned = true;
                db.SaveChanges();
            }
            // Skip Song
            else if(requestBody.ToLower() == "skip")
            {
                // TODO: Skip Current Song
            }
            // Song Selection
            else if (songSelection.IsMatch(requestBody))
            {
                SelectSong(requestBody, Phone.Parse(requestPhoneNumber));
            }
            // Help Host
            else if (requestBody.ToLower() == "help" && Phone.Parse(requestPhoneNumber) == db.EventUsers.Where(e => e.PhoneNumber == Phone.Parse(requestPhoneNumber) && e.UserID == db.Events.Where(ev => ev.HostID == e.UserID).Single().HostID && db.Events.Where(p => p.HostID == e.UserID).Single().IsHosted).Single().PhoneNumber)
            {
                // TODO: Help message
            }
            // Help User
            else if (requestBody.ToLower() == "help")
            {
                // TODO: Help Message For Users
            }
            // Queue
            else if(requestBody.ToLower() == "queue" && db.Events.Where(p => p.EventID == (db.EventUsers.Where(e => e.PhoneNumber == Phone.Parse(requestPhoneNumber)).Single().EventID)).Single().IsHosted == true)
            {
                var videos = db.Playlist;
                var messageString = "";
                var counter = 1;
                foreach(var video in videos)
                {
                    messageString += $"{counter}.) {video.Title}\r\n";
                    counter++;
                }
                MessagingResponse message = new MessagingResponse();
                message.Message();
                SendMessage(message);
            }
            // Hot Songs
            else if(requestBody.ToLower() == "hot")
            {
                // TODO: Send Message of Top Songs
            }
            // Like
            else if(requestBody.ToLower() == "like")
            {
                var currentSong = db.Playlist.First();
            }
            // Dislike
            else if(requestBody.ToLower() == "dislike")
            {
                // TODO: Dislike a Song
            }
            // Song Search
            else
            {
                var videos = await search.SearchByTitle(requestBody);
                VideosToMessage(videos);
                IdAndTitleToDB(videos, Phone.Parse(requestPhoneNumber));
            }
        }
        public void VideosToMessage(List<string[]> videos)
        {
            string titles = "";
            for(int i = 0; i < 5; i++)
            {
                titles += $"{i + 1}.) {videos[i][0]}\r\n";
            }
            MessagingResponse message = new MessagingResponse();
            message.Message(titles);
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
            person.Title1 = videos[1][0];
            person.Title1 = videos[2][0];
            person.Title1 = videos[3][0];
            person.Title1 = videos[4][0];
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
                    db.Playlist.Add(playlist);
                    break;
                case "2":
                    playlist.YoutubeID = person.Id2;
                    playlist.Title = person.Title2;
                    db.Playlist.Add(playlist);
                    break;
                case "3":
                    playlist.YoutubeID = person.Id3;
                    playlist.Title = person.Title3;
                    db.Playlist.Add(playlist);
                    break;
                case "4":
                    playlist.YoutubeID = person.Id4;
                    playlist.Title = person.Title4;
                    db.Playlist.Add(playlist);
                    break;
                case "5":
                    playlist.YoutubeID = person.Id5;
                    playlist.Title = person.Title5;
                    db.Playlist.Add(playlist);
                    break;
                default:
                    break;
            }
            db.SaveChanges();
            MessagingResponse message = new MessagingResponse();
            message.Message("Your song has been added to the queue.");
            SendMessage(message);
        }
        public void EventCode(string number, string message)
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
                    MessagingResponse returnMessage = new MessagingResponse();
                    returnMessage.Message("Sorry, The Event You're Looking For Is Either Not Currently Hosted Or The Event Code Entered Is Incorrect.");
                    SendMessage(returnMessage);
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
                    MessagingResponse welcomeToEventMessage = new MessagingResponse();
                    welcomeToEventMessage.Message($"You have been added to the event, {myEvent.EventName}.");
                    SendMessage(welcomeToEventMessage);
                }
            }

            // Returning User
            else if(user != default(EventUsers) && regex.IsMatch(message))
            {
                var myEvent = db.Events.Where(e => e.IsHosted == true && e.EventCode == message.ToLower()).SingleOrDefault();

                // EventHosted or Wrong Code
                if(myEvent == default(Events))
                {
                    MessagingResponse returnMessage = new MessagingResponse();
                    returnMessage.Message("Sorry, The Event You're Looking For Is Either Not Currently Hosted Or The Event Code Entered Is Incorrect.");
                    SendMessage(returnMessage);
                }

                // Event Hosted and Code Valid, Return User
                else
                {
                    
                    user.EventID = myEvent.EventID;
                    db.SaveChanges();
                    MessagingResponse welcomeToEventMessage = new MessagingResponse();
                    welcomeToEventMessage.Message($"You have been added to the event, {myEvent.EventName}.");
                    SendMessage(welcomeToEventMessage);
                }
            }
            else
            {
                MessagingResponse errorMessage = new MessagingResponse();
                errorMessage.Message("Invalid Input. Please try again, or Type 'help' for more information.");
                SendMessage(errorMessage);
            }
        }
        public ActionResult SendMessage(MessagingResponse message)
        {
            return TwiML(message);
        }
    }
}