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
            this.CheckEmp();
            this.CheckNoVote();
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
                .Where(u => u.Cedula
                .Equals("0123456789"))
                .FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    FirstName = "Marcos",
                    LastName  = "Banda",
                    Curso     = "NA",
                    Cedula    = "0123456789",
                    Photo     = "~/Security/Content/Photos/admin.jpg",
                };

                db.Users.Add(user);
                db.SaveChanges();

            }

            var userASP = userManager.FindByName(user.Cedula);
            if (userASP == null)
            {
                userASP = new ApplicationUser
                {
                    UserName = user.Cedula,
                    Email = user.Cedula,
                };

                userManager.Create(userASP, "123456");
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
                .Where(u => u.Cedula
                .Equals("0000000000"))
                .FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    FirstName = "Empatada",
                    LastName  = "Votaci�n",
                    Curso     = "NA",
                    Cedula    = "0000000000",
                    Photo     = "~/Security/Content/Photos/balance.png",
                };

                db.Users.Add(user);
                db.SaveChanges();

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
                .Where(u => u.Cedula
                .Equals("0000000001"))
                .FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    FirstName = "Nulo",
                    LastName  = "Voto",
                    Curso     = "NA",
                    Cedula    = "0000000001",
                    Photo     = "~/Security/Content/Photos/novote.png",
                };

                db.Users.Add(user);
                db.SaveChanges();

            }
        }

        private void CheckEmp()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var db = new DemocracyContext();

            this.CheckRole("Admin", userContext);
            this.CheckRole("User", userContext);

            var user = db.Users
                .Where(u => u.Cedula
                .Equals("0000000002"))
                .FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    FirstName = "En Blanco",
                    LastName = "Voto",
                    Curso = "NA",
                    Cedula = "0000000002",
                    Photo = "~/Security/Content/Photos/empyvote.png",
                };

                db.Users.Add(user);
                db.SaveChanges();

            }
        }

        private void CheckNoVote()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var db = new DemocracyContext();

            this.CheckRole("Admin", userContext);
            this.CheckRole("User", userContext);

            var user = db.Users
                .Where(u => u.Cedula
                .Equals("0000000003"))
                .FirstOrDefault();

            if (user == null)
            {
                user = new User
                {
                    FirstName = "No Sufragaron",
                    LastName = "Estudiantes Que",
                    Curso = "NA",
                    Cedula = "0000000003",
                    Photo = "~/Security/Content/Photos/noimage.png",
                };

                db.Users.Add(user);
                db.SaveChanges();

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
