using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TwilioProject
{
    class Twilio
    {
        public void SendMessage(string toNumber, string fromNumber, string messageBody)
        {
            const string accountSid = Keys.TwilioLiveSId;
            const string authToken = Keys.AuthTokenLive;
            TwilioClient.Init(accountSid, authToken);

            var to = new PhoneNumber(toNumber);
            var message = MessageResource.Create(
                to,
                from: new PhoneNumber(fromNumber),
                body: messageBody);
        }
    }
}