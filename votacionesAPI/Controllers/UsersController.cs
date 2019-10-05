using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using votacionesAPI.Classes;
using votacionesAPI.Models;

namespace votaciones.Controllers.API
{
    [RoutePrefix("api/Users")]
    public class UsersController : ApiController
    {
        private DemocracyContext db = new DemocracyContext();

        [Authorize]
        [Route("Login")]
        public IHttpActionResult Login(JObject form)
        {
            string email = string.Empty;
            string password = string.Empty;
            dynamic jsonObject = form;

            try
            {
                email = jsonObject.email.Value;
                password = jsonObject.password.Value;
            }
            catch
            {

                return this.BadRequest("Llamada incorrecta");
            }

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.Find(email, password);

            if (userASP == null)
            {
                return this.BadRequest("Usuario o contraseña incorrectos");
            }

            var user = db.Users
                .Where(u => u.UserName == email)
                .FirstOrDefault();

            if (user == null)
            {
                return this.BadRequest("better call saul");
            }

            return this.Ok(user);
        }

        [Authorize]
        [HttpPut]
        [Route("ChangePassword/{userId}")]
        public IHttpActionResult ChangePassword(int userId, JObject form)
        {
            var oldPassword = string.Empty;
            var newPassword = string.Empty;

            dynamic jsonObject = form;

            try
            {
                oldPassword = jsonObject.OldPassword.Value;
                newPassword = jsonObject.NewPassword.Value;
            }
            catch
            {

                return this.BadRequest("Llamada incorrecta");
            }

            var user = db.Users.Find(userId);
            if (user == null)
            {
                return this.BadRequest("Usuario no encontrado");
            }

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.Find(user.UserName, oldPassword);

            if (userASP == null)
            {
                return this.BadRequest("Contraseña antigua incorrecta");
            }

            var response = userManager.ChangePassword(userASP.Id, oldPassword, newPassword);
            if (response.Succeeded)
            {
                return this.Ok<object>(new
                {
                    Message = "La contraseña fue cambiada exitosamente"
                });
            }
            else
            {
                return this.BadRequest(response.Errors.ToString());
            }
        }

        // PUT: api/Users/5
        [Authorize]
        [HttpPut]
        public IHttpActionResult PutUser(int id, UserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != request.UserId)
            {
                return BadRequest();
            }

            if (request.ImageArray != null && request.ImageArray.Length > 0)
            {
                var stream = new MemoryStream(request.ImageArray);
                var guid = Guid.NewGuid().ToString();
                var file = string.Format("{0}.jpg", guid);
                var folder = "/Content/Photos";
                var folder2 = "~/Content/Photos";
                var fullPath = string.Format("{0}/{1}", folder, file);
                var fullPath2 = string.Format("~/Security{0}",fullPath);
                var response = FilesHelper.UploadPhoto(stream, folder2, file);

                if (response)
                {
                    request.Photo = fullPath2;
                }
            }

            var user = ToUser(request);
            db.Entry(user).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return this.Ok(user);
        }

        private User ToUser(UserRequest request)
        {
            return new User
            {
                Adress = request.Adress,
                Facultad = request.Facultad,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Photo = request.Photo,
                Cedula = request.Cedula,
                UserId = request.UserId,
                UserName = request.UserName,
            };
        }

        [HttpPost]
        [Route("PasswordRecovery")]
        public async Task<IHttpActionResult> PasswordRecovery(JObject form)
        {
            try
            {
                var email = string.Empty;
                dynamic jsonObject = form;

                try
                {
                    email = jsonObject.Email.Value;
                }
                catch
                {
                    return BadRequest("Incorrect call.");
                }

                var user = await db.Users
                    .Where(u => u.UserName.ToLower() == email.ToLower())
                    .FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound();
                }

                var userContext = new ApplicationDbContext();
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
                var userASP = userManager.FindByEmail(email);
                if (userASP == null)
                {
                    return NotFound();
                }

                var random = new Random();
                var newPassword = string.Format("{0}{1}{2:04}*", user.FirstName.ToUpper().Substring(0, 1), user.LastName.ToLower(), random.Next(9999));
                var response1 = userManager.RemovePassword(userASP.Id);
                var response2 = await userManager.AddPasswordAsync(userASP.Id, newPassword);
                if (response2.Succeeded)
                {
                    var subject = "Votaciones Utc Recuperar contraseña";
                    var body = string.Format(@"
                    <h1>Votaciones UTC Recuperar Contraseña</h1>
                    <p>Su nueva contraseña es: <strong>{0}</strong></p>
                    <p>Puede cambiar esta contraseña por una nueva que recuerde facilmente.",
                            newPassword);

                    await MailHelper.SendMail(email, subject, body);
                    return Ok(true);
                }

                return BadRequest("La contrasena no pudo ser cambiada.");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }
    }
}