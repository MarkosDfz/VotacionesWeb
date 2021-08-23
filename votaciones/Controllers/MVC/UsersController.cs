using CrystalDecisions.CrystalReports.Engine;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using votaciones.Classes;
using votaciones.Models;

namespace votaciones.Controllers
{
    public class UsersController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public string GetUserList(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            sSearch = sSearch.ToLower();
            int totalRecord = db.Users.Where(x => x.Cedula != "0000000000" && x.Cedula != "0000000001" && x.Cedula != "0000000002"
                                             && x.Cedula != "0000000003").Count();
            var usuarios = new List<User>();
            if (!string.IsNullOrEmpty(sSearch))
                usuarios = db.Users.Where(a => a.LastName.ToLower().Contains(sSearch)
                || a.FirstName.ToLower().Contains(sSearch)
                || a.Curso.ToLower().Contains(sSearch)
                || a.Cedula.StartsWith(sSearch)
                ).OrderBy(a => a.LastName).Skip(iDisplayStart).Take(iDisplayLength).ToList();
            else
                usuarios = db.Users.OrderBy(a => a.LastName).Skip(iDisplayStart).Take(iDisplayLength).ToList();

            var result = (from p in usuarios 
                          select new User
                          {
                              UserId   = p.UserId,
                              Curso    = p.Curso,
                              LastName = p.FullName,
                              Cedula   = p.Cedula,
                              Photo    = p.Photo.Replace("~", ""),
                          }
                         ).Where(x => x.Cedula != "0000000000" && x.Cedula != "0000000001" && x.Cedula != "0000000002"
                                 && x.Cedula != "0000000003").ToList();

            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("{");
            sb.Append("\"sEcho\": ");
            sb.Append(sEcho);
            sb.Append(",");
            sb.Append("\"iTotalRecords\": ");
            sb.Append(totalRecord);
            sb.Append(",");
            sb.Append("\"iTotalDisplayRecords\": ");
            sb.Append(totalRecord);
            sb.Append(",");
            sb.Append("\"aaData\": ");
            sb.Append(JsonConvert.SerializeObject(result));
            sb.Append("}");
            return sb.ToString();
        }

        public bool EsAdm(User user)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));

            var userASP = userManager.FindByEmail(user.Cedula);

            var rp = userASP != null && userManager.IsInRole(userASP.Id, "Admin");

            return rp;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ResetPass(int id)
        {
            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));

            var user = db.Users.Find(id);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Error");
                return RedirectToAction("Edit", new { id });
            }

            var userASP = userManager.FindByEmail(user.Cedula);

            if (userASP == null)
            {
                ModelState.AddModelError(string.Empty, "Error");
                return RedirectToAction("Edit", new { id });
            }

            userManager.RemovePassword(userASP.Id);
            userManager.AddPassword(userASP.Id, user.Cedula);

            TempData["DataPass"] = "* Contraseña reseteada correctamente";

            return RedirectToAction("Edit", new { id });
        }

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
        public ActionResult ImportData()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Import(HttpPostedFileBase excelfile)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                ViewBag.Error = "Por favor seleccione un archivo Excel<br/>";
                return View("Index");
            }
            else
            {
                if (excelfile.FileName.EndsWith("xls") || excelfile.FileName.EndsWith("xlsx"))
                {
                    string doc = string.Empty;
                    doc = Path.GetFileName(excelfile.FileName);
                    string path = Server.MapPath("~/Content/Data/" + doc);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                    excelfile.SaveAs(path);

                    // Leer datos del excel
                    var package = new ExcelPackage(new FileInfo(path));
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[1];
                    var range = workSheet.Dimension.Rows;
                    for (int row = 4; row <= range; row++)
                    {
                        User u = new User();

                        u.Cedula    = workSheet.Cells[row, 1].Value.ToString();
                        u.LastName  = workSheet.Cells[row, 2].Value.ToString();
                        u.FirstName = workSheet.Cells[row, 3].Value.ToString();
                        u.Curso     = workSheet.Cells[row, 4].Value.ToString();
                        u.Group     = null;
                        u.Photo     = "~/Security/Content/Photos/noimage.png";

                        var userview = new UserView
                        {
                            Cedula = u.Cedula,
                        };

                        var repetido = false;

                        var user = db.Users
                            .Where(nu => nu.Cedula == u.Cedula)
                            .FirstOrDefault();

                        if (user != null)
                        {
                            repetido = true;
                        }

                        if (!repetido)
                        {
                            db.Users.Add(u);

                            try
                            {
                                db.SaveChanges();
                                Utilities.CreateASPUser(userview);
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError(string.Empty, ex.Message);
                                return View("ImportData");
                            }
                        }
                    }

                    TempData["DataOk"] = "* Usuarios importados correctamente";
                    return View("ImportData");
                }
                else
                {
                    ViewBag.Error = "El tipo de archivo es incorrecto<br/>";
                    return View("ImportData");
                }
            }
        }

        [Authorize(Roles = "Admin")]
        public virtual ActionResult DownloadFile()
        {
            string fullPath = Path.Combine(Server.MapPath("~/Content/Formato/users.xlsx"));
            return File(fullPath, "application/octet-stream", "users.xlsx");
        }

        private ReportClass GenerateUserReport()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var dataTable = new DataTable();
            var sql = @"SELECT UserId, LastName + ' ' + FirstName 
                        AS     Estudiante, Cedula, Curso 
                        FROM   Users 
                        EXCEPT (SELECT UserId, LastName + ' ' + FirstName 
                        AS     Estudiante, Cedula, Curso FROM Users WHERE Cedula LIKE '000000000%')
                        ORDER BY Estudiante";

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
                .Where(u => u.Cedula == this.User.Identity.Name)
                .FirstOrDefault();
            var view = new UserSettingsView
            {
                FirstName = user.FirstName,
                Curso     = user.Curso,
                Group     = user.Group,
                LastName  = user.LastName,
                Cedula    = user.Cedula,
                Photo     = user.Photo,
                UserId    = user.UserId,
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
                string newGuid = Guid.NewGuid().ToString();

                if (view.NewPhoto != null)
                {
                    pic = String.Format("{0}.jpg", newGuid);
                    path = Path.Combine(Server.MapPath("~/Security/Content/Photos"), pic);

                    view.NewPhoto.SaveAs(path);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        view.NewPhoto.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                    }
                }

                var user = db.Users.Find(view.UserId);

                user.FirstName = view.FirstName;
                user.Curso     = view.Curso;
                user.Group     = view.Group;
                user.LastName  = view.LastName;
                user.Cedula    = view.Cedula;

                if (!string.IsNullOrEmpty(pic))
                {
                    user.Photo = string.Format("~/Security/Content/Photos/{0}", pic);
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
                var userASP = userManager.FindByEmail(user.Cedula);

                if (userASP != null)
                {
                    if (userManager.IsInRole(userASP.Id, "Admin"))
                    {
                        userManager.RemoveFromRole(userASP.Id, "Admin");
                        TempData["DataAdm"] = "* Este usuario ha dejado de ser Administrador";
                    }
                    else
                    {
                        userManager.AddToRole(userASP.Id, "Admin");
                        TempData["DataAdm"] = "* Este usuario ahora es Administrador";
                    }
                }
            }

            return RedirectToAction("Details", new { id });
        }

        // GET: Users/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));

            var user = db.Users.Find(id);
            var userASP = userManager.FindByEmail(user.Cedula);

            var userView = new UserIndexView
            {
                Candidates   = user.Candidates,
                Curso        = user.Curso,
                FirstName    = user.FirstName,
                Group        = user.Group,
                GroupMembers = user.GroupMembers,
                IsAdmin      = userASP != null && userManager.IsInRole(userASP.Id, "Admin"),
                LastName     = user.LastName,
                Cedula       = user.Cedula,
                Photo        = user.Photo,
                UserId       = user.UserId,
            };

            if (user == null)
            {
                return HttpNotFound();
            }
            return View(userView);
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
            string pic  = string.Empty;
            string newGuid = Guid.NewGuid().ToString();

            if (userView.Photo != null)
            {
                pic = String.Format("{0}.jpg", newGuid);
                path = Path.Combine(Server.MapPath("~/Security/Content/Photos"), pic);

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
                FirstName = userView.FirstName,
                Curso     = userView.Curso,
                Group     = userView.Group,
                LastName  = userView.LastName,
                Cedula    = userView.Cedula,
                Photo     = string.IsNullOrEmpty(pic) ? string.Format("~/Security/Content/Photos/noimage.png") : string.Format("~/Security/Content/Photos/{0}",pic),
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
                    ex.InnerException.InnerException.Message.Contains("CedulaIndex"))
                {
                    ModelState.AddModelError(string.Empty, "El número de celuda ingresado ya se encuentra registrado");
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
                FirstName = user.FirstName,
                Curso     = user.Curso,
                Group     = user.Group,
                LastName  = user.LastName,
                Cedula    = user.Cedula,
                UserId    = user.UserId,
            };

            List<SelectListItem> lst = new List<SelectListItem>();

            lst.Add(new SelectListItem() { Text = "PROYECTO NAP", Value = "PROYECTO NAP" });
            lst.Add(new SelectListItem() { Text = "2DO EGB",  Value = "2DO EGB" });
            lst.Add(new SelectListItem() { Text = "3RO EGB",  Value = "3RO EGB" });
            lst.Add(new SelectListItem() { Text = "4TO EGB",  Value = "4TO EGB" });
            lst.Add(new SelectListItem() { Text = "5TO EGB",  Value = "5TO EGB" });
            lst.Add(new SelectListItem() { Text = "6TO EGB",  Value = "6TO EGB" });
            lst.Add(new SelectListItem() { Text = "7MO EGB",  Value = "7MO EGB" });
            lst.Add(new SelectListItem() { Text = "8VO EGB",  Value = "8VO EGB" });
            lst.Add(new SelectListItem() { Text = "9NO EGB",  Value = "9NO EGB" });
            lst.Add(new SelectListItem() { Text = "10MO EGB", Value = "10MO EGB"});
            lst.Add(new SelectListItem() { Text = "1RO BACH", Value = "1RO BACH"});
            lst.Add(new SelectListItem() { Text = "2DO BACH", Value = "2DO BACH"});
            lst.Add(new SelectListItem() { Text = "3RO BACH", Value = "3RO BACH"});

            ViewBag.Curso = new SelectList(lst, "Value", "Text", user.Curso);

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
            string pic  = string.Empty;
            string newGuid = Guid.NewGuid().ToString();

            if (userView.Photo != null)
            {
                pic = String.Format("{0}.jpg", newGuid);
                path = Path.Combine(Server.MapPath("~/Security/Content/Photos"), pic);

                userView.Photo.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    userView.Photo.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
            }

            var user = db.Users.Find(userView.UserId);

            user.FirstName = userView.FirstName;
            user.Curso     = userView.Curso;
            user.Group     = userView.Group;
            user.LastName  = userView.LastName;
            user.Cedula    = userView.Cedula;

            if (!string.IsNullOrEmpty(pic))
            {
                user.Photo = string.Format("~/Security/Content/Photos/{0}", pic);
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
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            var userAsp = db.Users.Find(id).Cedula;

            if (user != null)
            {
                foreach (var item in user.VotingDetails.ToList())
                {
                    db.VotingDetails.Remove(item);
                }

                foreach (var item in user.GroupMembers.ToList())
                {
                    db.GroupMembers.Remove(item);
                }

                db.Users.Remove(user);
            }

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
