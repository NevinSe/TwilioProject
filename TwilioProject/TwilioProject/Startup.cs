using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TwilioProject.Models;
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
            CreateRolesAndUsers();
        }
        private void CreateRolesAndUsers()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            if (!roleManager.RoleExists("Host"))
            {
                var role = new IdentityRole();
                role.Name = "Host";
                roleManager.Create(role);
            }
            if (!roleManager.RoleExists("NotHost"))
            {
                var role = new IdentityRole();
                role.Name = "NotHost";
                roleManager.Create(role);
            }
        }
    }
}
