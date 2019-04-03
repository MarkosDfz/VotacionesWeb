using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using votaciones.Models;

namespace votaciones.Controllers
{
    public class StatesController : Controller
    {
        //con esto nos conectamos a la bd
        private DemocracyContext db = new DemocracyContext();

        // GET: States
        [HttpGet]
        public ActionResult Index()
        {
            //aqui devolvemos toda la lista de estados
            return View(db.States.ToList());
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(State state)
        {
            if (!ModelState.IsValid)
            {
                return View(state);
            }

            //para guardar en la bd los datos de estado
            db.States.Add(state);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //con esto cerramos la conexion a la bd
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}