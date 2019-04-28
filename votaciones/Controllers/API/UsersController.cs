﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using votaciones.Classes;
using votaciones.Models;

namespace votaciones.Controllers.API
{
    [RoutePrefix("api/Users")]
    public class UsersController : ApiController
    {
        private DemocracyContext db = new DemocracyContext();

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
            if (user ==  null)
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
        [HttpPut]
        public IHttpActionResult PutUser(int id, User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.UserId)
            {
                return BadRequest();
            }
            
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

        // POST: api/Users
        [HttpPost]
        public IHttpActionResult PostUser(RegisterUserView userView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User
            {
                Adress = userView.Adress,
                FirstName = userView.FirstName,
                Grade = userView.Grade,
                Group = userView.Group,
                LastName = userView.LastName,
                Phone = userView.Phone,
                UserName = userView.UserName,
            };

            db.Users.Add(user);
            db.SaveChanges();
            this.CreateASPUser(userView);

            return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        }

        private ApplicationUser CreateASPUser(RegisterUserView userView)
        {
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

            userManager.Create(userASP, userView.Password);

            userASP = userManager.FindByName(userView.UserName);
            userManager.AddToRole(userASP.Id, "User");
            return userASP;
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