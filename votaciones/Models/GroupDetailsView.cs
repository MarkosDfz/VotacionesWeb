using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class GroupDetailsView
    {
        public int GroupId { get; set; }

        public string Description { get; set; }

        public List<GroupMember> Members { get; set; }
    }

}