using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace votaciones.Models
{
    public class DemocracyContext : DbContext
    {
        //con este constructor conectamos a la bd
        public DemocracyContext() 
            : base("DefaultConnection")
        {

        }

        //con Dbset se van listando las tablas que se van a la db
        public DbSet<State> States { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Voting> Votings { get; set; }

        public DbSet<User> Users { get; set; }
    }
}