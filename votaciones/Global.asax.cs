using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
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
            this.CheckDraw();
            this.CheckNull();
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
                    Facultad = "CIYA",
                    Cedula = "0503962169",
                    UserName = "markosdefaz@gmail.com",
                    Photo = "~/Security/Content/Photos/admin.jpg",
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
                };

                userManager.Create(userASP, "Dfz666**");
            }

            userManager.AddToRole(userASP.Id, "Admin");
            
        }

        private void CheckDraw()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var db = new DemocracyContext();

            this.CheckRole("Admin", userContext);
            this.CheckRole("User", userContext);

            var user = db.Users
                .Where(u => u.UserName.ToLower()
                .Equals("empate@empate.com"))
                .FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    Adress = "Latacunga",
                    FirstName = "Empatada",
                    LastName = "Votación",
                    Facultad = "null",
                    Cedula = "0000000000",
                    UserName = "empate@empate.com",
                    Photo = "~/Security/Content/Photos/balance.png",
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
                };

                userManager.Create(userASP, user.UserName);
            }

        }

        private void CheckNull()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var db = new DemocracyContext();

            this.CheckRole("Admin", userContext);
            this.CheckRole("User", userContext);

            var user = db.Users
                .Where(u => u.UserName.ToLower()
                .Equals("votonulo"))
                .FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    Adress = "null",
                    FirstName = "Nulo",
                    LastName = "Voto",
                    Facultad = "null",
                    Cedula = "0000000000",
                    UserName = "votonulo",
                    Photo = "~/Security/Content/Photos/novote.png",
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
                };

                userManager.Create(userASP, user.UserName);
            }

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
