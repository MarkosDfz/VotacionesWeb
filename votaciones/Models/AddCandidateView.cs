using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class AddCandidateView
    {
        public int VotingId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un usuario")]
        public int[] UserId { get; set; }
    }
}