using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using votaciones.Models;

namespace votaciones.Classes
{
    public class Utilities : IDisposable
    {
        private static DemocracyContext db = new DemocracyContext();

        public static string UploadPhoto(HttpPostedFileBase file)
        {
            //Subir Imagen

            string path = string.Empty;
            string pic = string.Empty;

            if (file != null)
            {
                pic = Path.GetFileName(file.FileName);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Photos"), pic);
                file.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }

            return pic;
        }

        public static void CreateASPUser(UserView userView)
        {
            //Gestion de usuarios

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(userContext));

            //crear el rol del usuario

            string roleName = "User";

            //comprobar que el rol no existe sino lo creamos

            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }

            // creamos el usuario de ASPNET

            var userASP = new ApplicationUser
            {
                UserName = userView.UserName,
                Email = userView.UserName,
                PhoneNumber = userView.Phone,
            };

            userManager.Create(userASP, userASP.UserName);

            userASP = userManager.FindByName(userView.UserName);
            userManager.AddToRole(userASP.Id, "User");

        }

        public static void DeleteASPUser(string id)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));

            var user = (from p in userContext.Users
                           where p.Email == id
                           select p).FirstOrDefault();

            userManager.Delete(user);

        }

        public static void ChangeUserName(string currentUserName, UserChange user)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.FindByEmail(currentUserName);
            if (userASP == null)
            {
                return;
            }

            userManager.Delete(userASP);

            userASP = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.UserName,
                PhoneNumber = user.Phone,
            };

            userManager.Create(userASP, user.CurrentPassword);
            userManager.AddToRole(userASP.Id, "User");
        }

        public static List<Voting>MyVotings(User user)
        {
            //Obtener del evento de votaciones en el tiempo establecido
            var state = GetState("Abierta");

            var votings = db.Votings
                .Where(v => v.StateId == state.StateId &&
                            v.DateTimeStart <= DateTime.Now &&
                            v.DateTimeEnd >= DateTime.Now)
                            .Include(v => v.Candidates)
                            .Include(v => v.VotingGroups)
                            .Include(v => v.State)
                            .ToList();

            //Descartar eventos de votacion en el que el usuario ya voto
            foreach (var voting in votings.ToList())
            {

                var votingDetail = db.VotingDetails
                    .Where(vd => vd.VotingId == voting.VotingId &&
                                 vd.UserId == user.UserId)
                                 .FirstOrDefault();

                if (votingDetail != null)
                {
                    votings.Remove(voting);
                }
            }


            //descartar los eventos de votacion en los grupos que no pertenese el usuario
            foreach (var voting in votings.ToList())
            {
                if (!voting.IsForAllUsers)
                {
                    bool userBelongsToGroup = false;

                    foreach (var votingGroup in voting.VotingGroups)
                    {
                        var userGroup = votingGroup.Group.GroupMembers
                            .Where(gm => gm.UserId == user.UserId)
                            .FirstOrDefault();

                        if (userGroup != null)
                        {
                            userBelongsToGroup = true;
                            break;
                        }
                    }

                    if (!userBelongsToGroup)
                    {
                        votings.Remove(voting);
                    }
                }
            }

            return votings;
        }

        public static State GetState(string stateName)
        {
            var state = db.States.Where(s => s.Description == stateName).FirstOrDefault();
            if (state == null)
            {
                state = new State
                {
                    Description = stateName,
                };

                db.States.Add(state);
                db.SaveChanges();
            }

            return state;
        }

        public static async Task SendMail(string to, string subject, string body)
        {
            var message = new MailMessage();
            message.To.Add(new MailAddress(to));
            message.From = new MailAddress(WebConfigurationManager.AppSettings["AdminUser"]);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = WebConfigurationManager.AppSettings["AdminUser"],
                    Password = WebConfigurationManager.AppSettings["AdminPassWord"]
                };

                smtp.Credentials = credential;
                smtp.Host = WebConfigurationManager.AppSettings["SMTPName"];
                smtp.Port = int.Parse(WebConfigurationManager.AppSettings["SMTPPort"]);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
        }


        public static async Task PasswordRecovery(string email)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.FindByEmail(email);
            if (userASP == null)
            {
                return;
            }

            var user = db.Users.Where(tp => tp.UserName == email).FirstOrDefault();
            if (user == null)
            {
                return;
            }

            var random = new Random();
            var newPassword = string.Format("{0}{1}{2:04}*", user.FirstName.ToUpper().Substring(0, 1), user.LastName.ToLower(), random.Next(9999));

            userManager.RemovePassword(userASP.Id);
            userManager.AddPassword(userASP.Id, newPassword);

            var subject = "Votaciones Utc Recuperar contraseña";
            var body = string.Format(@"
            <h1>Votaciones UTC Recuperar Contraseña</h1>
            <p>Su nueva contraseña es: <strong>{0}</strong></p>
            <p>Puede cambiar esta contraseña por una nueva que recuerde facilmente.",
                    newPassword);

            await SendMail(email, subject, body);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}