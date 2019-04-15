using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    [NotMapped]
    public class UserChange : User
    {
        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(20, ErrorMessage =
            "El campo {0} puede contener un máximo de {1} y un mínimo de {2} de caracteres."
           , MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
    }
}