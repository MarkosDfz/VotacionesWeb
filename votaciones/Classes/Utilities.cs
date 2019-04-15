using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
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

        public void Dispose()
        {
            db.Dispose();
        }
    }
}