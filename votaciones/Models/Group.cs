using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    //esta clase "Group" es la tabla grupo con sus respectivos atributos
    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50, ErrorMessage =
            "El campo {0} debe contener un mínimo de {2} y un máximo de {1} de caracteres."
           , MinimumLength = 3)]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [JsonIgnore]
        public virtual ICollection<GroupMember> GroupMembers { get; set; }

        [JsonIgnore]
        public virtual ICollection<VotingGroup> VotingGroups { get; set; }

    }
}