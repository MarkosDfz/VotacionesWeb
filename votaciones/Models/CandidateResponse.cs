using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    [NotMapped]
    public class CandidateResponse
    {
        public int CandidateId { get; set; }

        public int QuantityVotes { get; set; }

        public virtual User User { get; set; }
    }
}