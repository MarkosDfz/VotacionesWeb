using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class VotingGroup
    {
        [Key]
        public int VotingGroupId { get; set; }

        public int VotingId { get; set; }

        public int GroupId { get; set; }

        [JsonIgnore]
        public virtual Voting Voting { get; set; }

        [JsonIgnore]
        public virtual Group Group { get; set; }
    }
}