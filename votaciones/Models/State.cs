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
        public int StateId { get; set; }

        public string Description { get; set; }
    }
}