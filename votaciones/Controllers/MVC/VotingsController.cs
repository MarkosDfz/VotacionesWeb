using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using votaciones.Classes;
using votaciones.Models;

namespace votaciones.Controllers
{
    public class VotingsController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Close(int id)
        {
            var voting = db.Votings.Find(id);
            if (voting != null)
            {
                var candidate = db.Candidates
                    .Where(c => c.VotingId == voting.VotingId)
                    .OrderByDescending(c => c.QuantityVotes)
                    .FirstOrDefault();

                if (candidate != null)
                {
                    var state = Utilities.GetState("Cerrada");
                    voting.StateId = state.StateId;
                    voting.CandidateWinId = candidate.User.UserId;

                    db.Entry(voting).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult ShowResults(int id)
        {
            var report = this.GenerateResultReport(id);
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        private ReportClass GenerateResultReport(int id)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var dataTable = new DataTable();
            var sql = @"SELECT Votings.VotingId, Votings.Description AS Voting, States.Description AS State, 
                               Users.FirstName + ' ' + Users.LastName AS Candidate, Candidates.QuantityVotes
                        FROM   Candidates INNER JOIN
                               Users ON Candidates.UserId = Users.UserId INNER JOIN
                               Votings ON Candidates.VotingId = Votings.VotingId INNER JOIN
                               States ON Votings.StateId = States.StateId
                        WHERE  Votings.VotingId = " + id + " ORDER BY Candidates.QuantityVotes DESC";

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
            report.FileName = Server.MapPath("/Reports/Results.rpt");
            report.Load();
            report.SetDataSource(dataTable);
            return report;
        }


        public ActionResult Results()
        {
            var state = Utilities.GetState("Cerrada");
            var votings = db.Votings
                .Where(v => v.StateId == state.StateId)
                .Include(v => v.State);
            var views = new List<VotingIndexView>();
            var db2 = new DemocracyContext();
            foreach (var voting in votings)
            {
                User user = null;
                if (voting.CandidateWinId != 0)
                {
                    user = db2.Users.Find(voting.CandidateWinId);
                }

                views.Add(new VotingIndexView
                {
                    CandidateWinId = voting.CandidateWinId,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsEnableBlankVote = voting.IsEnableBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = voting.QuantityBlankVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    State = voting.State,
                    VotingId = voting.VotingId,
                    Winner = user,
                });
            }
            return View(views);

        }

        [Authorize(Roles = "User")]
        public ActionResult VoteForCandidate(int candidateId, int votingId)
        {
            var user = db.Users
                .Where(u => u.UserName == this.User.Identity.Name)
                .FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Index","Home");
            }

            var candidate = db.Candidates.Find(candidateId);
            if (candidate == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var voting = db.Votings.Find(votingId);
            if (voting == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (this.VoteCandidate(user, candidate, voting))
            {
                return RedirectToAction("MyVotings");
            }

            return RedirectToAction("Index", "Home");
        }

        private bool VoteCandidate(User user, Candidate candidate, Voting voting)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                var votingDetail = new VotingDetail
                {
                    CandidateId = candidate.CandidateId,
                    DateTime = DateTime.Now,
                    UserId = user.UserId,
                    VotingId = voting.VotingId,
                };

                db.VotingDetails.Add(votingDetail);

                candidate.QuantityVotes++;
                db.Entry(candidate).State = EntityState.Modified;

                voting.QuantityVotes++;
                db.Entry(voting).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }

            return false;
        }

        [Authorize(Roles = "User")]
        public ActionResult Vote(int votingId)
        {
            var voting = db.Votings.Find(votingId);
            var view = new VotingVoteView
            {
                DateTimeEnd = voting.DateTimeEnd,
                DateTimeStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnableBlankVote = voting.IsEnableBlankVote,
                IsForAllUsers = voting.IsForAllUsers,
                MyCandidates = voting.Candidates.ToList(),
                Remarks = voting.Remarks,
                VotingId = voting.VotingId,
            };

            return View(view);
        }

        [Authorize(Roles = "User")]
        public ActionResult MyVotings()
        {
            var user = db.Users.Where(u => u.UserName == this.User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Existe un error con el usuario actual, contacte con soporte.");
                return View();
            }

            var votings = Utilities.MyVotings(user);
            return View(votings);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteGroup(int id)
        {
            var votingGroup = db.VotingGroups.Find(id);
            if (votingGroup != null)
            {
                db.VotingGroups.Remove(votingGroup);
                db.SaveChanges();

            }

            return RedirectToAction(string.Format("Details/{0}", votingGroup.VotingId));
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCandidate(int id)
        {
            //nota importante, aqui no me muestra el error en la app cuando ya se ha votado
            var candidate = db.Candidates.Find(id);
            if (candidate != null)
            {
                db.Candidates.Remove(candidate);

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

                        ModelState.AddModelError(string.Empty, "El registro no puede ser eliminado porque tiene registros relacionados");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }

                }
            }

            return RedirectToAction(string.Format("Details/{0}", candidate.VotingId));
        }

        [HttpPost]
        public ActionResult AddCandidate(AddCandidateView view)
        {
            if (ModelState.IsValid)
            {
                var candidate = db.Candidates
                    .Where(c => c.VotingId == view.VotingId &&
                                c.UserId == view.UserId)
                    .FirstOrDefault();

                if (candidate != null)
                {
                    ModelState.AddModelError(string.Empty, "El candidato ya pertenece a la votación");
                    ViewBag.UserId = new SelectList(db.Users
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName), "UserId", "FullName");

                    return View(view);
                }

                candidate = new Candidate
                {
                    UserId = view.UserId,
                    VotingId = view.VotingId,

                };

                db.Candidates.Add(candidate);
                db.SaveChanges();
                return RedirectToAction(string.Format("Details/{0}", view.VotingId));

            }

            ViewBag.UserId = new SelectList(db.Users
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName), "UserId", "FullName");

            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddCandidate(int id)
        {
            var view = new AddCandidateView
            {
                VotingId = id,

            };

             ViewBag.UserId = new SelectList(db.Users
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName), "UserId", "FullName");

            return View(view);
        }

        [HttpPost]
        public ActionResult AddGroup(AddGroupView view)
        {
            if (ModelState.IsValid)
            {
                var votingGroup = db.VotingGroups
                    .Where(vg => vg.VotingId == view.VotingId &&
                                 vg.GroupId == view.GroupId)
                    .FirstOrDefault();

                if (votingGroup != null)
                {
                    ModelState.AddModelError(string.Empty, "El grupo ya pertenece a la votación");
                    ViewBag.GroupId = new SelectList(
                    db.Groups.OrderBy(g => g.Description),
                    "GroupId",
                    "Description");

                    return View(view);
                }

                votingGroup = new VotingGroup
                {
                    GroupId = view.GroupId,
                    VotingId = view.VotingId,

                };

                db.VotingGroups.Add(votingGroup);
                db.SaveChanges();
                return RedirectToAction(string.Format("Details/{0}", view.VotingId));

            }

            ViewBag.GroupId = new SelectList(
                db.Groups.OrderBy(g => g.Description),
                "GroupId",
                "Description");

            return View(view);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddGroup(int id)
        {
            ViewBag.GroupId = new SelectList(
                db.Groups.OrderBy(g => g.Description), 
                "GroupId", 
                "Description");

            var view = new AddGroupView
            {
                VotingId = id,
            };

            return View(view);
        }

        // GET: Votings
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var votings = db.Votings.Include(v => v.State);
            var views = new List<VotingIndexView>();
            var db2 = new DemocracyContext();
            foreach (var voting in votings)
            {
                User user = null;
                if (voting.CandidateWinId != 0)
                {
                    user = db2.Users.Find(voting.CandidateWinId);
                }

                views.Add(new VotingIndexView
                {
                    CandidateWinId = voting.CandidateWinId,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsEnableBlankVote = voting.IsEnableBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = voting.QuantityBlankVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    State = voting.State,
                    VotingId = voting.VotingId,
                    Winner = user,
                });
            }
            return View(views);
        }

        // GET: Votings/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }

            var view = new DetailsVotingView
            {
                Candidates = voting.Candidates.ToList(),
                CandidateWinId = voting.CandidateWinId,
                DateTimeStart = voting.DateTimeStart,
                DateTimeEnd = voting.DateTimeEnd,
                Description = voting.Description,
                IsEnableBlankVote = voting.IsEnableBlankVote,
                IsForAllUsers = voting.IsForAllUsers,
                QuantityVotes = voting.QuantityVotes,
                QuantityBlankVotes = voting.QuantityBlankVotes,
                Remarks = voting.Remarks,
                StateId = voting.StateId,
                State = voting.State,
                VotingGroups = voting.VotingGroups.ToList(),
                VotingId = voting.VotingId,

            };

            return View(view);
        }

        // GET: Votings/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.StateId = new SelectList(db.States, "StateId", "Description");
            var view = new VotingView
            {
                DateStart = DateTime.Now,
                DateEnd = DateTime.Now,
            };

            return View(view);
        }

        // POST: Votings/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VotingView view)
        {
            if (ModelState.IsValid)
            {
                var voting = new Voting
                {
                    DateTimeEnd = view.DateEnd
                                      .AddHours(view.TimeEnd.Hour)
                                      .AddMinutes(view.TimeEnd.Minute),
                    DateTimeStart = view.DateStart
                                      .AddHours(view.TimeStart.Hour)
                                      .AddMinutes(view.TimeStart.Minute),
                    Description = view.Description,

                    IsEnableBlankVote = view.IsEnableBlankVote,

                    IsForAllUsers = view.IsForAllUsers,

                    Remarks = view.Remarks,

                    StateId = view.StateId,

                };

                db.Votings.Add(voting);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States, "StateId", "Description", view.StateId);
            return View(view);
        }

        // GET: Votings/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var voting = db.Votings.Find(id);

            if (voting == null)
            {
                return HttpNotFound();
            }

            var view = new VotingView
            {
                DateEnd = voting.DateTimeEnd,
                DateStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnableBlankVote = voting.IsEnableBlankVote,
                IsForAllUsers = voting.IsForAllUsers,
                Remarks = voting.Remarks,
                StateId = voting.StateId,
                TimeEnd = voting.DateTimeEnd,
                TimeStart = voting.DateTimeStart,
                VotingId = voting.VotingId,

            };

            ViewBag.StateId = new SelectList(db.States, "StateId", "Description", voting.StateId);
            return View(view);
        }

        // POST: Votings/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VotingView view)
        {
            if (ModelState.IsValid)
            {
                var voting = new Voting
                {
                    DateTimeEnd = view.DateEnd
                                      .AddHours(view.TimeEnd.Hour)
                                      .AddMinutes(view.TimeEnd.Minute),
                    DateTimeStart = view.DateStart
                                      .AddHours(view.TimeStart.Hour)
                                      .AddMinutes(view.TimeStart.Minute),
                    Description = view.Description,

                    IsEnableBlankVote = view.IsEnableBlankVote,

                    IsForAllUsers = view.IsForAllUsers,

                    Remarks = view.Remarks,

                    StateId = view.StateId,

                    VotingId = view.VotingId,

                };

                db.Entry(voting).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.StateId = new SelectList(db.States, "StateId", "Description", view.StateId);
            return View(view);
        }

        // GET: Votings/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }
            return View(voting);
        }

        // POST: Votings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Voting voting = db.Votings.Find(id);
            db.Votings.Remove(voting);
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
                    ModelState.AddModelError(string.Empty, "El registro no puede ser eliminado porque tiene registros relacionados");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                return View(voting);
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
