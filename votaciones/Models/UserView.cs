using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class UserView
    {
        public int UserId { get; set; }

        [Display(Name = "E-Mail")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(100, ErrorMessage =
            "El campo {0} puede contener un maximo de {1} y un minimo de {2} de carcteres."
           , MinimumLength = 7)]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Display(Name = "Nombres")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage =
            "El campo {0} puede contener un maximo de {1} y un minimo de {2} de carcteres."
           , MinimumLength = 3)]
        public string FirstName { get; set; }

        [Display(Name = "Apellidos")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage =
            "El campo {0} puede contener un maximo de {1} y un minimo de {2} de carcteres."
           , MinimumLength = 3)]
        public string LastName { get; set; }

        [Display(Name = "Celular")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(10, ErrorMessage =
            "El campo {0} puede contener un maximo de {1} y un minimo de {2} de carcteres."
           , MinimumLength = 8)]
        public string Phone { get; set; }

        [Display(Name = "Dirección")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(100, ErrorMessage =
            "El campo {0} puede contener un maximo de {1} y un minimo de {2} de carcteres."
           , MinimumLength = 5)]
        public string Adress { get; set; }

        [Display(Name = "Curso")]
        public string Grade { get; set; }

        [Display(Name = "Grupo")]
        public string Group { get; set; }

        [Display(Name = "Foto")]
        public HttpPostedFileBase Photo { get; set; }

    }
}