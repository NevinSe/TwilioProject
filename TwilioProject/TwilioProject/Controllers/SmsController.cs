using System.Web.Mvc;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using System.Linq;
using System.Data;
using TwilioProject.Models;
using System.Text.RegularExpressions;

namespace TwilioProject.Controllers
{
    public class SmsController : TwilioController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private Regex regex = new Regex(@"[A-Z0-9a-z]{5}");
        private Regex songSelection = new Regex(@"[1-5]");
        private YoutubeSearch search = new YoutubeSearch();

        [HttpPost]
        public void Index()
        {
            var requestPhoneNumber = Request.Form["From"];
            var requestBody = Request.Form["Body"];

            // Event Code
            if (regex.IsMatch(requestBody))
            {
                CheckPhone(requestPhoneNumber, requestBody);
            }
            // Song Selection
            else if (songSelection.IsMatch(requestBody))
            {
                SelectSong();
            }
            else if (requestBody.ToLower() == "help")
            {
                // TODO: Help Message
            }
            // Song Search
            else
            {
                // TODO: Nevin Search For Song API Work
            }
        }
        public void SelectSong()
        {

        }
        public void CheckPhone(string number, string message)
        {
            // Get Phone Number into format
            string phoneNumber = "";
            for(int i = 2; i < number.Length; i++)
            {
                phoneNumber += number[i];
            }
            int userPhone = int.Parse(phoneNumber);

            
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
                        EventID = myEvent.EventID,
                        NumbOfMessages = 1
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
                    user.NumbOfMessages = 1;
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