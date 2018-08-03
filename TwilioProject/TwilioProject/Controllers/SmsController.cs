using System.Web.Mvc;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using TwilioProject;
public class SmsController : TwilioController
{
    [HttpPost]
    public ActionResult Index()
    {
        var requestPhoneNumber = Request.Form["From"];
        var requestBody = Request.Form["Body"];
        var response = new MessagingResponse();
        if (requestBody == "hello")
        {
            response.Message("Hi!");
        }
        else if (requestBody == "bye")
        {
            response.Message("Goodbye");
        }
        return TwiML(response);
    }
}