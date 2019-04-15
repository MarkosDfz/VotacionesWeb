using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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