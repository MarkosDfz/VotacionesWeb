using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votacionesAPI.Models
{
    public class UserView
    {
        public int UserId { get; set; }

        [Display(Name = "E-Mail")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(100, ErrorMessage =
            "El campo {0} puede contener un máximo de {1} y un mínimo de {2} de caracteres."
           , MinimumLength = 7)]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Display(Name = "Nombres")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage =
            "El campo {0} puede contener un máximo de {1} y un mínimo de {2} de caracteres."
           , MinimumLength = 3)]
        public string FirstName { get; set; }

        [Display(Name = "Apellidos")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage =
            "El campo {0} puede contener un máximo de {1} y un mínimo de {2} de caracteres."
           , MinimumLength = 3)]
        public string LastName { get; set; }

        [Display(Name = "Cédula")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(13, ErrorMessage =
            "El campo {0} puede contener un máximo de {1} y un mínimo de {2} de caracteres."
           , MinimumLength = 10)]
        public string Cedula { get; set; }

        [Display(Name = "Dirección")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(100, ErrorMessage =
            "El campo {0} puede contener un máximo de {1} y un mínimo de {2} de caracteres."
           , MinimumLength = 5)]
        public string Adress { get; set; }

        [Display(Name = "Facultad")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Facultad { get; set; }

        [Display(Name = "Grupo")]
        public string Group { get; set; }

        [Display(Name = "Foto")]
        public HttpPostedFileBase Photo { get; set; }

    }
}