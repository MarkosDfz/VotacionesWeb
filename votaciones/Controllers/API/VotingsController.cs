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

        [HttpGet]
        [Route("{userId}")]
        public IHttpActionResult MyVotings(int userId)
        {
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return this.BadRequest("Usuario no encontrado");
            }

            var votings = Utilities.MyVotings(user);
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
                    IsEnableBlankVote = voting.IsEnableBlankVote,
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
            var db2 = new DemocracyContext();

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
                    IsEnableBlankVote = voting.IsEnableBlankVote,
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
            }
            base.Dispose(disposing);
        }
    }

}