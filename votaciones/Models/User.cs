using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Display(Name = "Cédula")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(13, ErrorMessage =
            "El campo {0} debe contener un mínimo de {2} y un máximo de {1} de caracteres."
           , MinimumLength = 10)]
        [Index("CedulaIndex", IsUnique = true)]
        public string Cedula { get; set; }

        [Display(Name = "Nombres")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage =
            "El campo {0} debe contener un mínimo de {2} y un máximo de {1} de caracteres."
           , MinimumLength = 3)]
        public string FirstName { get; set; }

        [Display(Name = "Apellidos")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage =
            "El campo {0} debe contener un mínimo de {2} y un máximo de {1} de caracteres."
           , MinimumLength = 3)]
        public string LastName { get; set; }

        [Display(Name = "Usuario")]
        public string FullName { get { return string.Format ("{0} {1}", this.LastName, this.FirstName) ; } }

        [Display(Name = "Grupo")]
        public string Group { get; set; }

        [Display(Name = "Curso")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Curso { get; set; }

        [Display(Name = "Foto")]
        [DataType(DataType.ImageUrl)]
        public string Photo { get; set; }

        [JsonIgnore]
        public virtual ICollection<GroupMember> GroupMembers { get; set; }

        [JsonIgnore]
        public virtual ICollection<Candidate> Candidates { get; set; }

        [JsonIgnore]
        public virtual ICollection<VotingDetail> VotingDetails { get; set; }
    }
}