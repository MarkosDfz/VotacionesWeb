using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using votaciones.Models;

namespace votaciones.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GroupsController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        [HttpGet]
        public ActionResult DeleteMember(int id)
        {
            var member = db.GroupMembers.Find(id);
            if (member != null)
            {
                db.GroupMembers.Remove(member);
                db.SaveChanges();
            }

            return RedirectToAction(string.Format("Details/{0}", member.GroupId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMember(AddMemberView view)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = new SelectList(db.Users
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName), "UserId", "FullName");
                return View(view);
            }

            foreach (var user in view.UserId)
            {
                var member = db.GroupMembers
                    .Where(gm => gm.GroupId == view.GroupId && gm.UserId == user)
                    .FirstOrDefault();

                var repetido = false;

                if (member != null)
                {
                    if ( view.UserId.Count() == 1 )
                    {
                        ViewBag.UserId = new SelectList(db.Users.Where(x => x.UserName != "votacionempatada" && x.UserName != "votonulo")
                        .OrderBy(u => u.FirstName)
                        .ThenBy(u => u.LastName), "UserId", "FullName");
                        ModelState.AddModelError(string.Empty, "El miembro ya pertenece al grupo");
                        return View(view);
                    }
                    else
                    {
                        repetido = true;
                    }
                }
            
                if (!repetido)
                {
                    member = new GroupMember
                    {
                        GroupId = view.GroupId,
                        UserId = user,
                    };

                    db.GroupMembers.Add(member);
                    db.SaveChanges();
                }
                
            }

            return RedirectToAction(string.Format("Details/{0}",view.GroupId));

        }


        [HttpGet]
        public ActionResult AddMember(int groupId)
        {
            ViewBag.UserId = new SelectList(db.Users.Where(x => x.UserName != "votacionempatada" && x.UserName != "votonulo")
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName), "UserId", "FullName");
            var view = new AddMemberView
            {
                GroupId = groupId,
            }; 

            return View(view);
        }

        // GET: Groups
        public ActionResult Index()
        {
            return View(db.Groups.ToList());
        }

        // GET: Groups/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }

            var view = new GroupDetailsView
            {
                GroupId = group.GroupId,
                Description = group.Description,
                Members = group.GroupMembers.ToList(),
            };

            return View(view);
        }

        // GET: Groups/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GroupId,Description")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Groups.Add(group);

                var gr = db.Groups
                    .Where(g => g.Description == group.Description)
                    .FirstOrDefault();

                var gru = db.Groups.Count();

                if ( gru > 0)
                {
                    if (gr.Description == group.Description)
                    {
                        ModelState.AddModelError(string.Empty, "El grupo ya existe");
                        return View();
                    }
                }

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(group);
        }

        // GET: Groups/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "GroupId,Description")] Group group)
        {
            if (ModelState.IsValid)
            {
                db.Entry(group).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(group);
        }

        // GET: Groups/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Group group = db.Groups.Find(id);
            db.Groups.Remove(group);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null &&
                    ex.InnerException.InnerException != null &&
                    ex.InnerException.InnerException.Message.Contains("REFERENCE"))
                {
                    ModelState.AddModelError(string.Empty, "No se puede borrar el registro porque tiene valores relacionados");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                return View(group);
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
