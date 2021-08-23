using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class AddGroupView
    {
        public int VotingId { get; set; }

        [Required(ErrorMessage = "Se Debe Elegir un grupo")]
        public int[] GroupId { get; set; }
    }
}