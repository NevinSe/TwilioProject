using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TwilioProject
{
    class Twilio
    {
        public Twilio()
        {
            Test();
        }
        public void Test()
        {
            // Find your Account Sid and Auth Token at twilio.com/console
            const string accountSid = Keys.TwilioLiveSId;
            const string authToken = Keys.AuthTokenLive;
            TwilioClient.Init(accountSid, authToken);

            var to = new PhoneNumber("+12625271771");
            var message = MessageResource.Create(
                to,
                from: new PhoneNumber("+19842057019"),
                body: "This is the ship that made the Kessel Run in fourteen parsecs?");

            Console.WriteLine(message.Sid);
        }
    }
}