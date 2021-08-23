using CrystalDecisions.CrystalReports.Engine;
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

namespace votaciones.Controllers
{
    public class VotingsController : Controller
    {
        private DemocracyContext db = new DemocracyContext();
        private DemocracyContext db2 = new DemocracyContext();
        private DemocracyContext db3 = new DemocracyContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Propuesta()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ImportPropuestas(HttpPostedFileBase pdffile)
        {
            if (pdffile == null || pdffile.ContentLength == 0)
            {
                ViewBag.Error = "Por favor seleccione un archivo Pdf<br/>";
                return View("Index");
            }
            else
            {
                if (pdffile.FileName.EndsWith("pdf"))
                {
                    string doc = string.Empty;
                    doc = "propuesta.pdf";
                    string path = Server.MapPath("~/Content/Data/" + doc);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                    pdffile.SaveAs(path);

                    TempData["DataPdf"] = "* Propuesta importada correctamente";
                    return View("Propuesta");
                }
                else
                {
                    ViewBag.Error = "El tipo de archivo es incorrecto<br/>";
                    return View("Propuesta");
                }
            }
        }

        public virtual ActionResult DownloadPropuesta()
        {
            string fullPath = Path.Combine(Server.MapPath("~/Content/Data/propuesta.pdf"));
            return File(fullPath, "application/octet-stream", "propuesta.pdf");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Close(int id)
        {
            var voting = db3.Votings.Find(id);

            
                var us = db3.Users.Where(u => u.Cedula == "0000000003").FirstOrDefault();

                if (us != null)
                {
                    int[] use = new int[1];
                    use[0] = us.UserId;

                    var ccs = new AddCandidateView
                    {
                        UserId = use,
                        VotingId = voting.VotingId
                    };

                    AddCandidate(ccs);

                    var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    var connection = new SqlConnection(connectionString);
                    var sql = @"SELECT Count(LastName) - 4 FROM Users
                                WHERE  Users.UserId NOT IN (SELECT VotingDetails.UserId
                                FROM    Votings INNER JOIN VotingDetails ON Votings.VotingId = VotingDetails.VotingId
                                INNER JOIN Users ON VotingDetails.UserId = Users.UserId WHERE Votings.VotingId = " + voting.VotingId + ") EXCEPT (SELECT Count(LastName) - 4 FROM Users WHERE Cedula LIKE '00000000%')";

                    connection.Open();
                    var command = new SqlCommand(sql, connection);
                    var reader = command.ExecuteReader();
                    reader.Read();
                    var s = reader.GetInt32(0);
                    connection.Close();

                    var usw = db3.Candidates.Where(u => u.User.Cedula == "0000000003" && u.VotingId == voting.VotingId).FirstOrDefault();

                    usw.QuantityVotes = s;

                    db3.Entry(usw).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (voting != null)
                {
                    var candidate = db3.Candidates
                        .Where(c => c.VotingId == voting.VotingId && c.User.Cedula != "0000000003")
                        .OrderByDescending(c => c.QuantityVotes)
                        .FirstOrDefault();

                    if (voting.Candidates.Count > 1)
                    {
                        var candidate1 = db3.Candidates
                                .Where(c => c.VotingId == voting.VotingId && c.User.Cedula != "0000000003")
                                .OrderByDescending(c => c.QuantityVotes).Take(2).Skip(1).FirstOrDefault();

                        if (candidate.QuantityVotes == candidate1.QuantityVotes)
                        {
                            var state = Utilities.GetState("Cerrada");
                            voting.StateId = state.StateId;

                            var draw = db3.Users
                            .Where(c => c.Cedula == "0000000000")
                            .FirstOrDefault();
                            voting.CandidateWinId = draw.UserId;

                            db3.Entry(voting).State = EntityState.Modified;
                            db3.SaveChanges();
                            return RedirectToAction("Index");
                        }
                    }

                    if (candidate != null)
                    {
                        var state = Utilities.GetState("Cerrada");
                        voting.StateId = state.StateId;
                        voting.CandidateWinId = candidate.User.UserId;

                        db3.Entry(voting).State = EntityState.Modified;
                        db3.SaveChanges();
                        TempData["candi"] = candidate.User.UserId;
                    }
                }
            

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ShowCursoResults(int id)
        {
            var report = this.GenerateCursoResults(id);
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        private ReportClass GenerateCursoResults(int id)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var dataTable = new DataTable();
            var sql = @"SELECT votings.votingid, candidates.candidateid, users.lastname + ' ' + users.firstname AS Candidato, us2.curso,
                        COUNT  (us2.curso) AS votantes FROM Votings 
                        INNER JOIN VotingDetails ON Votings.VotingId = VotingDetails.VotingId
                        INNER JOIN candidates ON VotingDetails.candidateid = candidates.candidateid
                        INNER JOIN users ON users.userid = candidates.userid 
                        INNER JOIN users us2 ON  VotingDetails.userid = us2.userid
                        WHERE votings.votingid = " + id +
                        "GROUP BY votings.votingid, candidates.candidateid, users.lastname, users.firstname, us2.curso ORDER BY votantes DESC";

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
            report.FileName = Server.MapPath("/Reports/Curso.rpt");
            report.Load();
            report.SetDataSource(dataTable);
            return report;
        }

        [Authorize(Roles = "User")]
        public ActionResult MyCertificates()
        {
            var user = db.Users.Where(u => u.Cedula == this.User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Existe un error con el usuario actual, contacte con soporte.");
                return View();
            }

            var certificate = db.VotingDetails.Where(v => v.UserId == user.UserId).ToList();


            return View(certificate);
        }

        public ActionResult ShowCertificate1(int id, int id2)
        {
            var report = this.GenerateCertificateReport(id, id2);
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf", "certificado.pdf");
        }

        [HttpPost]
        public ActionResult ShowCertificate(int id, int id2)
        {
            var report = this.GenerateCertificateReport(id, id2);
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        private ReportClass GenerateCertificateReport(int id, int id2)
        {
            
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var dataTable = new DataTable();
            var sql = @"SELECT   Votings.VotingId, Votings.Description, Votings.DateTimeStart, Votings.DateTimeEnd, Users.UserId, Users.Cedula, Users.LastName +' '+ Users.FirstName AS Users
                        FROM     Users INNER JOIN
                                 VotingDetails ON Users.UserId = VotingDetails.UserId INNER JOIN
                                 Votings ON VotingDetails.VotingId = Votings.VotingId
                        WHERE    Votings.VotingId =" + id + "AND Users.UserId =" + id2;

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
            report.FileName = Server.MapPath("/Reports/Certificate.rpt");
            report.Load();
            report.SetDataSource(dataTable);
            return report;
        }

        public ActionResult ShowResults(int id)
        {
            var report = this.GenerateResultReport(id);
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        private ReportClass GenerateResultReport(int id)
        {
            var us = db.Candidates.Where(c => c.VotingId == id && c.User.Cedula == "0000000003").FirstOrDefault();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var dataTable = new DataTable();
            var sql = @"SELECT Votings.VotingId, Votings.Description AS Voting, States.Description AS State, 
                               Users.LastName + ' ' + Users.FirstName AS Candidate, Candidates.QuantityVotes,
                               Count(Us2.FirstName) - 4 As Num, Count(Candidates.QuantityVotes) - " + us.QuantityVotes + " - 4 As Votos FROM   Users As Us2, Candidates INNER JOIN Users ON Candidates.UserId = Users.UserId INNER JOIN Votings ON Candidates.VotingId = Votings.VotingId INNER JOIN States ON Votings.StateId = States.StateId WHERE  Votings.VotingId = " + id + " GROUP BY Votings.VotingId, Votings.Description, States.Description, Candidates.QuantityVotes, Users.LastName , Users.FirstName ORDER BY Candidates.QuantityVotes DESC";

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

        [Authorize(Roles = "Admin")]
        public ActionResult ShowNoVote(int id)
        {
            var voting = db.Votings.Where(v => v.VotingId == id).FirstOrDefault();

            if (voting.IsForAllUsers)
            {
                var report = this.GenerateNoVoteReport(id);
                var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                return File(stream, "application/pdf");
            }
            else
            {
                var report = this.GenerateNoVoteGroupReport(id);
                var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                return File(stream, "application/pdf");
            }
        }

        private ReportClass GenerateNoVoteReport(int id)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var dataTable = new DataTable();
            var sql = @"SELECT LastName + ' ' + FirstName AS Estudiante, Cedula, Curso FROM Users
                        WHERE  Users.UserId NOT IN (SELECT VotingDetails.UserId
                        FROM    Votings INNER JOIN VotingDetails ON Votings.VotingId = VotingDetails.VotingId
                        INNER JOIN Users ON VotingDetails.UserId = Users.UserId WHERE Votings.VotingId = " + id + ")" +
                        "EXCEPT (SELECT LastName + ' ' + FirstName AS Estudiante, Cedula, Curso FROM Users WHERE Cedula LIKE '000000000%') ORDER BY Estudiante";

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
            report.FileName = Server.MapPath("/Reports/NoVote.rpt");
            report.Load();
            report.SetDataSource(dataTable);
            return report;
        }

        private ReportClass GenerateNoVoteGroupReport(int id)
        {
            var s = string.Join(",", db.VotingGroups.Where(v => v.VotingId == id)
                                 .Select(v => v.GroupId.ToString()));

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var dataTable = new DataTable();
            var sql = @"SELECT users.userid, LastName + ' ' + FirstName AS Estudiante, Cedula, Curso
                        FROM   Users INNER JOIN GroupMembers ON GroupMembers.UserId = Users.UserId 
                        WHERE  Users.UserId NOT IN (SELECT VotingDetails.UserId 
                        FROM   Votings INNER JOIN VotingDetails ON Votings.VotingId = VotingDetails.VotingId
                        INNER JOIN Users ON VotingDetails.UserId = Users.UserId
                        WHERE Votings.VotingId = " + id + ") AND GroupMembers.groupid IN (" + s + ") ORDER BY Estudiante";
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
            report.FileName = Server.MapPath("/Reports/NoVote.rpt");
            report.Load();
            report.SetDataSource(dataTable);
            return report;
        }


        public ActionResult Results()
        {
            var state = Utilities.GetState("Cerrada");
            var state2 = Utilities.GetState("Abierta");


            var votings = db2.Votings
                .Where(v => v.StateId == state.StateId)
                .Include(v => v.State);

            var votings2 = db2.Votings
                .Where(v => v.StateId == state2.StateId)
                .Include(v => v.State);

            foreach (var voting in votings2)
            {
                if (voting.StateId == state2.StateId)
                {
                    var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

                    if (voting.DateTimeEnd < time)
                    {
                        Close(voting.VotingId);
                        voting.StateId = state.StateId;
                        voting.State = state;
                        voting.CandidateWinId = Int32.Parse(TempData["candi"].ToString());
                    }
                }
            }

            var views = new List<VotingIndexView>();
            
            foreach (var voting in votings)
            {
                User user = null;

                if (voting.CandidateWinId != 0)
                {
                    user = db.Users.Find(voting.CandidateWinId);
                }

                views.Add(new VotingIndexView
                {
                    CandidateWinId = voting.CandidateWinId,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityVotes = voting.QuantityVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    State = voting.State,
                    VotingId = voting.VotingId,
                    Winner = user,
                });
            }
            return View(views);

        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult VoteForCandidate(int candidateId, int votingId)
        {
            var user = db.Users
                .Where(u => u.Cedula == this.User.Identity.Name)
                .FirstOrDefault();

            if (user == null)
            {
                return RedirectToAction("Index","Home");
            }

            var successvote = db.VotingDetails
                .Where(vd => vd.UserId == user.UserId && vd.VotingId == votingId)
                .FirstOrDefault();

            if (successvote != null)
            {
                return View("Error");
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
                return View("SuccessfulVote");
            }
            
            return RedirectToAction("Index", "Home");
        }

        private bool VoteCandidate(User user, Candidate candidate, Voting voting)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

                var votingDetail = new VotingDetail
                {
                    CandidateId = candidate.CandidateId,
                    DateTime = time,
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

        [HttpPost]
        [Authorize(Roles = "User")]
        public ActionResult Vote(int votingId)
        {
            var voting = db.Votings.Find(votingId);
            var view = new VotingVoteView
            {
                DateTimeEnd = voting.DateTimeEnd,
                DateTimeStart = voting.DateTimeStart,
                Description = voting.Description,
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
            var user = db.Users.Where(u => u.Cedula == this.User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Existe un error con el usuario actual, contacte con soporte.");
                return View();
            }

            var state = Utilities.GetState("Abierta");
            var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

            var votings = db.Votings
                .Where(v => v.StateId == state.StateId &&
                            v.DateTimeStart <= time &&
                            v.DateTimeEnd >= time)
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
                        TempData["Errors"] = "* El candidato no puede ser eliminado porque ya cuenta con votos registrados";
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
                foreach (var user in view.UserId)
                {
                    var candidate = db.Candidates
                        .Where(c => c.VotingId == view.VotingId &&
                                    c.UserId == user)
                        .FirstOrDefault();

                    var repetido = false;

                    if (candidate != null)
                    {
                        if ( view.UserId.Count() == 1 )
                        {
                            ModelState.AddModelError(string.Empty, "El candidato ya pertenece a la votación");
                            ViewBag.UserId = new SelectList(db.Users.Where(x => x.Cedula != "0000000000"
                                                                           && x.Cedula != "0000000003")
                            .OrderBy(u => u.FirstName)
                            .ThenBy(u => u.LastName), "UserId", "FullName");
                            return View(view);
                        }
                        else
                        {
                            repetido = true;
                        }
                    }

                    if (!repetido)
                    {
                        candidate = new Candidate
                        {
                            UserId = user,
                            VotingId = view.VotingId,
                        };

                        db.Candidates.Add(candidate);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction(string.Format("Details/{0}", view.VotingId));

            }

            ViewBag.UserId = new SelectList(db.Users.Where(x => x.Cedula != "0000000000"
                                                           && x.Cedula != "0000000003")
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

             ViewBag.UserId = new SelectList(db.Users.Where(x => x.Cedula != "0000000000"
                                                            && x.Cedula != "0000000003")
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName), "UserId", "FullName");

            return View(view);
        }

        [HttpPost]
        public ActionResult AddGroup(AddGroupView view)
        {
            if (ModelState.IsValid)
            {
                foreach (var group in view.GroupId)
                {
                    var votingGroup = db.VotingGroups
                    .Where(vg => vg.VotingId == view.VotingId &&
                                 vg.GroupId == group)
                    .FirstOrDefault();

                    var repetido = false;

                    if (votingGroup != null)
                    {
                        if (view.GroupId.Count() == 1)
                        {
                            ModelState.AddModelError(string.Empty, "El grupo ya pertenece a la votación");
                            ViewBag.GroupId = new SelectList(db.Groups.OrderBy(g => g.Description),
                            "GroupId","Description");
                            return View(view);
                        }
                        else
                        {
                            repetido = true;
                        }
                    }

                    if (!repetido)
                    {
                        votingGroup = new VotingGroup
                        {
                            GroupId = group,
                            VotingId = view.VotingId,
                        };

                        db.VotingGroups.Add(votingGroup);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction(string.Format("Details/{0}", view.VotingId));

            }

            ViewBag.GroupId = new SelectList(
                db.Groups.OrderBy(g => g.Description),
                "GroupId","Description");

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
            var state = Utilities.GetState("Abierta");
            var state2 = Utilities.GetState("Cerrada");
            var db2 = new DemocracyContext();
            foreach (var voting in votings)
            {
                User user = null;

                if (voting.StateId == state.StateId)
                {
                    var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

                    if (voting.DateTimeEnd < time)
                    {
                        Close(voting.VotingId);
                        voting.StateId = state2.StateId;
                        voting.State = state2;
                        voting.CandidateWinId = Int32.Parse(TempData["candi"].ToString());
                    }
                }

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
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityVotes = voting.QuantityVotes,
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
            var db2 = new DemocracyContext();
            if (voting == null)
            {
                return HttpNotFound();
            }
            User user = null;
            if (voting.CandidateWinId != 0)
            {
                user = db2.Users.Find(voting.CandidateWinId);
            }
            var view = new DetailsVotingView
            {
                Candidates = voting.Candidates.Where(x => x.User.Cedula != "0000000003").ToList(),
                CandidateWinId = voting.CandidateWinId,
                Nombre = user,
                DateTimeStart = voting.DateTimeStart,
                DateTimeEnd = voting.DateTimeEnd,
                Description = voting.Description,
                IsForAllUsers = voting.IsForAllUsers,
                QuantityVotes = voting.QuantityVotes,
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
            var time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
            
            ViewBag.StateId = new SelectList(db.States, "StateId", "Description");
            
            var view = new VotingView
            {
                DateStart = time,
                DateEnd = time,
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

            if (voting.State.Description == "Cerrada")
            {
                return View("Error2");
            }

            var view = new VotingView
            {
                DateEnd = voting.DateTimeEnd,
                DateStart = voting.DateTimeStart,
                Description = voting.Description,
                QuantityVotes = voting.QuantityVotes,
                IsForAllUsers = voting.IsForAllUsers,
                Remarks = voting.Remarks,
                StateId = voting.StateId,
                TimeEnd = voting.DateTimeEnd,
                TimeStart = voting.DateTimeStart,
                VotingId = voting.VotingId,
                CandidateWinId = voting.CandidateWinId,
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
                   
                    IsForAllUsers = view.IsForAllUsers,

                    Remarks = view.Remarks,

                    StateId = view.StateId,

                    QuantityVotes = view.QuantityVotes,

                    VotingId = view.VotingId,

                    CandidateWinId = view.CandidateWinId,
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
                db2.Dispose();
                db3.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
