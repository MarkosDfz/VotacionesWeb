using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    [NotMapped]
    public class UserIndexView : User
    {

        [Display(Name = "¿Es Admin?")]
        public bool IsAdmin { get; set; }

    }
}