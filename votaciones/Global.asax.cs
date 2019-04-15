using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using votaciones.Migrations;
using votaciones.Models;

namespace votaciones
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DemocracyContext, Configuration>());
            this.CheckSuperUser();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private void CheckSuperUser()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var db = new DemocracyContext();

            this.CheckRole("Admin", userContext);
            this.CheckRole("User", userContext);

            var user = db.Users
                .Where(u => u.UserName.ToLower()
                .Equals("markosdefaz@gmail.com"))
                .FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    Adress = "Latacunga",
                    FirstName = "Marcos",
                    LastName = "Banda",
                    Phone = "0983430874",
                    UserName = "markosdefaz@gmail.com",
                    Photo = "~/Content/Photos/admin.jpg",
                };

                db.Users.Add(user);
                db.SaveChanges();

            }

            var userASP = userManager.FindByName(user.UserName);
            if (userASP == null)
            {
                userASP = new ApplicationUser
                {
                    UserName = user.UserName,
                    Email = user.UserName,
                    PhoneNumber = user.Phone,
                };

                userManager.Create(userASP, "Dfz666**");
            }

            userManager.AddToRole(userASP.Id, "Admin");
            userManager.AddToRole(userASP.Id, "User");
        }

        private void CheckRole(string roleName, ApplicationDbContext userContext)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));

            //comprobar que el rol no existe sino lo creamos

            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }
        }
    }
}
