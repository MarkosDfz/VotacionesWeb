using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using votacionesAPI.Models;

namespace votacionesAPI.Classes
{
    public class Utilities : IDisposable
    {
        private static DemocracyContext db = new DemocracyContext();

        public static State GetState(string stateName)
        {
            var state = db.States.Where(s => s.Description == stateName).FirstOrDefault();
            if (state == null)
            {
                state = new State
                {
                    Description = stateName,
                };

                db.States.Add(state);
                db.SaveChanges();
            }

            return state;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}