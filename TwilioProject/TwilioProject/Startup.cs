using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TwilioProject.Startup))]
namespace TwilioProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
