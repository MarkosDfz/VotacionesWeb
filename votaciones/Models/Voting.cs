using Newtonsoft.Json;
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
            "El campo {0} puede contener un máximo de {1} y un mínimo de {2} de caracteres."
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
        [Display(Name = "Fecha de Finalización")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm }", ApplyFormatInEditMode = true)]
        public DateTime DateTimeEnd { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "¿Para todos los usuarios?")]
        public bool IsForAllUsers { get; set; }

        //[Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "¿Están habilitados los votos en blanco?")]
        public bool IsEnableBlankVote { get; set; }

        [Display(Name = "Cantidad de votos")]
        public int QuantityVotes { get; set; }

        [Display(Name = "Cantidad de votos en blanco")]
        public int QuantityBlankVotes { get; set; }

        [Display(Name = "Ganador")]
        public int CandidateWinId { get; set; }

        //relacion de 1 - * entre voting y estate
        [JsonIgnore]
        public virtual State State { get; set; }

        [JsonIgnore]
        public virtual ICollection<VotingGroup> VotingGroups { get; set; }

        [JsonIgnore]
        public virtual ICollection<Candidate> Candidates { get; set; }

        [JsonIgnore]
        public virtual ICollection<VotingDetail> VotingDetails { get; set; }

    }
}