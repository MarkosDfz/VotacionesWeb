﻿using System;
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
        public int StateId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(50,ErrorMessage =
            "El campo {0} puede contener un maximo de {1} y un minimo de {2} de carcteres."
           ,MinimumLength = 3)]
        public string Description { get; set; }
    }
}