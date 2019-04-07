using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
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

        //validacion para que si existe un dato guardado en una tabla y hay relacion
        //no se borre en cascada
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        }

        //con Dbset se van listando las tablas que se van a la db
        public DbSet<State> States { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Voting> Votings { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<GroupMember> GroupMembers { get; set; }

        public DbSet<VotingGroup> VotingGroups { get; set; }
    }
}