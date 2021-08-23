using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace votacionesAPI.Models
{
    [NotMapped]
    public class UserRequest : User
    {
        public byte[] ImageArray { get; set; }
    }
}