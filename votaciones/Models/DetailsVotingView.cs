using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class DetailsVotingView
    {
        public int VotingId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage =
            "El campo {0} debe contener un mínimo de {2} y un máximo de {1} de caracteres."
           , MinimumLength = 3)]
        [Display(Name = "Nombre de la votación")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Estado")]
        public int StateId { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción de la votación")]
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
        [Display(Name = "¿Para todos los usuarios?")]
        public bool IsForAllUsers { get; set; }

        [Display(Name = "Cantidad de votos")]
        public int QuantityVotes { get; set; }

        [Display(Name = "Ganador")]
        public int CandidateWinId { get; set; }

        [NotMapped]
        public User Nombre { get; set; }

        //relacion de 1 - * entre voting y estate
        public State State { get; set; }

        public List<VotingGroup> VotingGroups { get; set; }

        public List<Candidate> Candidates { get; set; }
    }
}