using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using votaciones.Classes;
using votaciones.Models;

namespace votaciones.Controllers.API
{
    [RoutePrefix("api/Votings")]
    public class VotingsController : ApiController
    {
        private DemocracyContext db = new DemocracyContext();
        private DemocracyContext db2 = new DemocracyContext();

        [HttpGet]
        [Route("{userId}")]
        public IHttpActionResult MyVotings(int userId)
        {
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return this.BadRequest("Usuario no encontrado");
            }

            var state = Utilities.GetState("Abierta");

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
            var votingResponse = new List<VotingResponse>();
            foreach (var voting in votings)
            {
                User winner = null;
                if (voting.CandidateWinId != 0)
                {
                    winner = db.Users.Find(voting.CandidateWinId);
                }

                var candidates = new List<CandidateResponse>();
                foreach (var candidate in voting.Candidates)
                {
                    candidates.Add(new CandidateResponse
                    {
                        CandidateId = candidate.CandidateId,
                        QuantityVotes = candidate.QuantityVotes,
                        User = candidate.User,
                    });
                }

                votingResponse.Add(new VotingResponse
                {
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    Remarks = voting.Remarks,
                    IsForAllUsers = voting.IsForAllUsers,
                    Candidates = candidates,
                    State = voting.State,
                    VotingId = voting.VotingId,
                    Winner = winner,
                });
            }

            return this.Ok(votingResponse);
        }

        [HttpPost]
        [Route("{userId}/{candidateId}/{votingId}")]
        public IHttpActionResult VoteCandidate(int userId, int candidateId, int votingId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = db.Users.Find(userId);
            if (user == null)
            {
                return this.BadRequest("Usuario no encontrado");
            }

            var candidate = db.Candidates.Find(candidateId);
            if (user == null)
            {
                return this.BadRequest("Candidato no encontrado");
            }

            var voting = db.Votings.Find(votingId);
            if (user == null)
            {
                return this.BadRequest("Usuario no encontrado");
            }
            
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

                db.SaveChanges();
            

            return this.Ok(votingDetail);
        }

        [HttpGet]
        public IHttpActionResult Results()
        {
            var state = Utilities.GetState("Cerrada");
            var votings = db.Votings
                .Where(v => v.StateId == state.StateId)
                .Include(v => v.State);
            var votingResponse = new List<VotingResponse>();
          
            foreach (var voting in votings)
            {
                User winner = null;
                if (voting.CandidateWinId != 0)
                {
                    winner = db2.Users.Find(voting.CandidateWinId);
                }

                var candidates = new List<CandidateResponse>();
                foreach (var candidate in voting.Candidates)
                {
                    candidates.Add(new CandidateResponse
                    {
                        CandidateId = candidate.CandidateId,
                        QuantityVotes = candidate.QuantityVotes,
                        User = candidate.User,
                    });
                }

                votingResponse.Add(new VotingResponse
                {
                    Remarks = voting.Remarks,
                    QuantityVotes = voting.QuantityVotes,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsForAllUsers = voting.IsForAllUsers,
                    Candidates = candidates,
                    State = voting.State,
                    VotingId = voting.VotingId,
                    Winner = winner,
                });
            }

            return this.Ok(votingResponse);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                db2.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}