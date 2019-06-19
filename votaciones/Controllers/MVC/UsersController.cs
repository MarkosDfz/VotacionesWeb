﻿using CrystalDecisions.CrystalReports.Engine;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using votaciones.Classes;
using votaciones.Models;
using PagedList;

namespace votaciones.Controllers
{
    public class UsersController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        [Authorize(Roles = "Admin")]
        public ActionResult PDF()
        {
            var report = this.GenerateUserReport();
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult XLS()
        {
            var report = this.GenerateUserReport();
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.Excel);
            return File(stream, "application/xls,", "Usuarios.xls");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DOC()
        {
            var report = this.GenerateUserReport();
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.WordForWindows);
            return File(stream, "application/doc,", "Usuarios.doc");
        }

        private ReportClass GenerateUserReport()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var dataTable = new DataTable();
            var sql = "SELECT * FROM Users ORDER BY LastName, FirstName";

            try
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            var report = new ReportClass();
            report.FileName = Server.MapPath("/Reports/Users.rpt");
            report.Load();
            report.SetDataSource(dataTable);
            return report;

        }

        [Authorize(Roles = "User")]
        public ActionResult MySettings()
        {
            var user = db.Users
                .Where(u => u.UserName == this.User.Identity.Name)
                .FirstOrDefault();
            var view = new UserSettingsView
            {
                Adress = user.Adress,
                FirstName = user.FirstName,
                Group = user.Group,
                LastName = user.LastName,
                Cedula = user.Cedula,
                Photo = user.Photo,
                UserId = user.UserId,
                UserName = user.UserName,
            };

            return View(view);
        }

        [HttpPost]
        public ActionResult MySettings(UserSettingsView view)
        {
            if (ModelState.IsValid)
            {
                //Subir Imagen

                string path = string.Empty;
                string pic = string.Empty;

                if (view.NewPhoto != null)
                {
                    pic = Path.GetFileName(view.NewPhoto.FileName);
                    path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                    view.NewPhoto.SaveAs(path);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        view.NewPhoto.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                    }
                }

                var user = db.Users.Find(view.UserId);

                user.Adress = view.Adress;
                user.FirstName = view.FirstName;
                user.Group = view.Group;
                user.LastName = view.LastName;
                user.Cedula = view.Cedula;

                if (!string.IsNullOrEmpty(pic))
                {
                    user.Photo = string.Format("~/Content/Photos/{0}", pic);
                }

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "Home");

            }

            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult OnOffAdmin(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                var userContext = new ApplicationDbContext();
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
                var userASP = userManager.FindByEmail(user.UserName);

                if (userASP != null)
                {
                    if (userManager.IsInRole(userASP.Id, "Admin"))
                    {
                        userManager.RemoveFromRole(userASP.Id, "Admin");
                    }
                    else
                    {
                        userManager.AddToRole(userASP.Id, "Admin");
                    }
                }
            }

            return RedirectToAction("Index");
        }

        // GET: Users
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var users = db.Users.ToList();
            var usersView = new List<UserIndexView>();

            foreach (var user in users)
            {
                var userASP = userManager.FindByEmail(user.UserName);

                usersView.Add(new UserIndexView
                {
                    Adress = user.Adress,
                    Candidates = user.Candidates,
                    FirstName = user.FirstName,
                    Group = user.Group,
                    GroupMembers = user.GroupMembers,
                    IsAdmin = userASP != null && userManager.IsInRole(userASP.Id, "Admin"), 
                    LastName = user.LastName,
                    Cedula = user.Cedula,
                    Photo = user.Photo,
                    UserId = user.UserId,
                    UserName = user.UserName,
                });
            }
          
            return View(usersView);
        }

        // GET: Users/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserView userView)
        {
            if (!ModelState.IsValid)
            {
                return View(userView);
            }

            //Subir Imagen

            string path = string.Empty;
            string pic = string.Empty;

            if (userView.Photo != null)
            {
                pic = Path.GetFileName(userView.Photo.FileName);
                path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                userView.Photo.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    userView.Photo.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }
            
            //Guardar registro

            var user = new User
            {
                Adress = userView.Adress,
                FirstName = userView.FirstName,
                Group = userView.Group,
                LastName = userView.LastName,
                Cedula = userView.Cedula,
                Photo = pic == string.Empty ? string.Format("~/Content/Photos/noimage.png") : string.Format("~/Content/Photos/{0}",pic),
                UserName = userView.UserName,
            };

            db.Users.Add(user);

            try
            {
                db.SaveChanges();
                Utilities.CreateASPUser(userView);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null &&
                    ex.InnerException.InnerException != null && 
                    ex.InnerException.InnerException.Message.Contains("UserNameIndex"))
                {
                    ModelState.AddModelError(string.Empty, "El e-mail ya esta registrado");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                return View(userView);
            }

            return RedirectToAction("Index");

        }

        

        // GET: Users/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var userView = new UserView
            {
                Adress = user.Adress,
                FirstName = user.FirstName,
                Group = user.Group,
                LastName = user.LastName,
                Cedula = user.Cedula,
                UserId = user.UserId,
                UserName = user.UserName,
            };

            return View(userView);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserView userView)
        {
            if (!ModelState.IsValid)
            {
                return View(userView);
            }

            //Subir Imagen

            string path = string.Empty;
            string pic = string.Empty;

            if (userView.Photo != null)
            {
                pic = Path.GetFileName(userView.Photo.FileName);
                path = Path.Combine(Server.MapPath("~/Content/Photos"), pic);
                userView.Photo.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    userView.Photo.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }

            var user = db.Users.Find(userView.UserId);

            user.Adress = userView.Adress;
            user.FirstName = userView.FirstName;
            user.Group = userView.Group;
            user.LastName = userView.LastName;
            user.Cedula = userView.Cedula;

            if (!string.IsNullOrEmpty(pic))
            {
                user.Photo = string.Format("~/Content/Photos/{0}", pic);
            }

            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");

        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            var userAsp = db.Users.Find(id).UserName;
            db.Users.Remove(user);
            try
            {
                db.SaveChanges();
                Utilities.DeleteASPUser(userAsp);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null &&
                    ex.InnerException.InnerException != null &&
                    ex.InnerException.InnerException.Message.Contains("REFERENCE"))
                {
                    ModelState.AddModelError(string.Empty, "El registro no puede ser eliminado porque tiene valores relacionados");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                return View(user);
            }
            
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
