using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace votacionesAPI.Models
{

    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Display(Name = "E-Mail")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(100, ErrorMessage =
            "El campo {0} puede contener un máximo de {1} y un mínimo de {2} de caracteres."
           , MinimumLength = 7)]
        [DataType(DataType.EmailAddress)]
        [Index("UserNameIndex", IsUnique = true)]
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

        [Display(Name = "Usuario")]
        public string FullName { get { return string.Format("{0} {1}", this.LastName, this.FirstName); } }

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

        [Display(Name = "Grupo")]
        public string Group { get; set; }

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