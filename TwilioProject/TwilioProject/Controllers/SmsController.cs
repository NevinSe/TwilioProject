using System.Data.Entity;
using System.Web.Mvc;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using TwilioProject;
using TwilioProject.Models;

public class SmsController : TwilioController
{
    private ApplicationDbContext db;
    public SmsController()
    {
        db = new ApplicationDbContext();
    }
    [HttpPost]
    public void Index()
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


        

        sendMessage(response);
    }
    public ActionResult sendMessage(MessagingResponse message)
    {
        return TwiML(message);
    }
}