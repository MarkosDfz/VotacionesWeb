using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    //esta clase "State" es la tabla estado con sus respectivos atributos
    public class State
    {
        [Key]
        [Display(Name = "Estado")]
        public int StateId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50,ErrorMessage =
            "El campo {0} debe contener un mínimo de {2} y un máximo de {1} de caracteres."
           , MinimumLength = 3)]
        [Display(Name = "Estado de la votación")]
        public string Description { get; set; }

        //relacion de 1 - * entre voting y estate
        [JsonIgnore]
        public virtual ICollection<Voting> Votings { get; set; }

    }
}