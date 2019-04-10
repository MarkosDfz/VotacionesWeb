using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    [NotMapped]
    public class UserSettingsView : User
    {
        [Display(Name = "Nueva Foto")]
        public HttpPostedFileBase NewPhoto { get; set; }
    }
}