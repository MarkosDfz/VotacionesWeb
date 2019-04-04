using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class Voting
    {
        [Key]
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
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm }", ApplyFormatInEditMode = true)]
        public DateTime DateTimeStart { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Fecha de Finalizacion")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm }", ApplyFormatInEditMode = true)]
        public DateTime DateTimeEnd { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Esta habilitado para todos los usuarios?")]
        public bool IsForAllUsers { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Esta habilitado los votos en blanco?")]
        public bool IsEnableBlanlVote { get; set; }

        [Display(Name = "Cantidad de votos")]
        public int QuantityVotes { get; set; }

        [Display(Name = "Cantidad de votos en blanco")]
        public int QuantityBlankVotes { get; set; }

        [Display(Name = "Ganador")]
        public int CandidateWinId { get; set; }

        //relacion de 1 - * entre voting y estate
        public virtual State State { get; set; }
    }
}