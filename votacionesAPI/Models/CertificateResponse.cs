using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace votacionesAPI.Models
{
    public class CertificateResponse
    {
        public int VotingId { get; set; }

        public string Description { get; set; }

        public string Remarks { get; set; }

        public DateTime DateTimeStart { get; set; }

        public DateTime DateTimeEnd { get; set; }
    }
}