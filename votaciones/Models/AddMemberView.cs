using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class AddMemberView
    {
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Se Debe Elegir un usuario")]
        public int UserId { get; set; }
    }
}