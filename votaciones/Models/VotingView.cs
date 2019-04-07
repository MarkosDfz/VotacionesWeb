using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class VotingView
    {
        public int VotingId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage =
            "El campo {0} puede contener un maximo de {1} y un minimo de {2} de carcteres."
           , MinimumLength = 3)]
        [Display(Name = "Nombre de la votacion")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Estado")]
        public int StateId { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripcion de la votacion")]
        public string Remarks { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime DateStart { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Hora de Inicio")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm }", ApplyFormatInEditMode = true)]
        public DateTime TimeStart { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Fecha de Finalización")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime DateEnd { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Hora de Finalización")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm }", ApplyFormatInEditMode = true)]
        public DateTime TimeEnd { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Esta habilitado para todos los usuarios?")]
        public bool IsForAllUsers { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Esta habilitado los votos en blanco?")]
        public bool IsEnableBlankVote { get; set; }
    }
}