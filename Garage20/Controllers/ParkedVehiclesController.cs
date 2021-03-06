﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Garage20.DataAccess;
using Garage20.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace Garage20.Controllers
{
    public class ParkedVehiclesController : Controller
    {
        private GarageContext db = new GarageContext();

        // GET: ParkedVehicles
        public ActionResult Index()
        {
            ViewBag.VehicleTypeId = new SelectList(db.VehicleType, "Id", "VehicleTypeName");
            return View(db.ParkedVehicles.ToList());
        }
        public ActionResult SearchVehicle()
        {
            return View();
        }
        /*The search method. Allows you to search for any vehicle with a RegNr*/
        public ActionResult Search(string Search)
        {
            
            var result = db.ParkedVehicles.Where(v => v.RegNr == Search);
            ViewBag.Searched = "eftersomboolfunkarej";
            if (!result.Any())
            {
                if (Search != "")
                {
                    ViewBag.Description = "Kunde inte hitta fordonet med RegNr: " + Search;
                }
                else
                {
                    ViewBag.Description = "Vänligen ange ett registreringsnummer";
                }
                return View("Index", result?.ToList());
            }

            return View("Index", result.ToList());
        }

        /*The search method. Allows you to search for any vehicle with a RegNr*/
        public ActionResult CheckOut(string Search)
        {
            var result = db.ParkedVehicles.Where(v => v.RegNr == Search);
            if (!result.Any())
            {
                if (Search != "")
                {
                    ViewBag.Description = "Det finns inget fordon med registreringsnummer" + Search;
                }
                else
                {
                    ViewBag.Description = "Vänligen ange ett registreringsnummer";
                }
                
                return View("SearchVehicle");
            }

            return Receipt(result?.First()?.Id);
        }

        //public ActionResult Verify(string Verify)
        //{
        //    var vehicles = db.ParkedVehicles.Where(v => v.Verification == Verify);
        //    if (vehicles.Any())
        //    {
        //        db.ParkedVehicles.Remove(vehicles.First());
        //        db.SaveChanges();
        //    }
        //    return RedirectToAction("Index");
        //}

        // GET: ParkedVehicles/Details/5
        public ActionResult Receipt(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParkedVehicle parkedVehicle = db.ParkedVehicles.Find(id);
            if (parkedVehicle == null)
            {
                return HttpNotFound();
            }

            parkedVehicle.CheckOutTime = DateTime.Parse(DateTime.Now.ToString("g"));
            TimeSpan? ParkingDuration = parkedVehicle.CheckOutTime - parkedVehicle.CheckInTime;
            parkedVehicle.AmountFee = 5 * (int)Math.Ceiling(ParkingDuration?.TotalMinutes / 10 ?? 0);


            db.ParkedVehicles.Remove(parkedVehicle);
            db.SaveChanges();

            return View("Receipt",parkedVehicle);
        }

        // GET: ParkedVehicles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ParkedVehicles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        /* AmountFee is no longer editable. It will not be calculated automatically from Views > ParkedVehicles > Index, Line: 71 (Linus)*/
        public ActionResult Create([Bind(Include = "Id,RegNr,Color,Brand,Model,WheelsAmount,VehicleType,CheckInTime")] ParkedVehicle parkedVehicle)
        {
            var vehicle = db.ParkedVehicles.Where(v => v.RegNr == parkedVehicle.RegNr);
            if (ModelState.IsValid && !vehicle.Any())
            {
                /*CheckInTime is now being defined by the user's current time when the user parks a car (Linus)*/
                parkedVehicle.CheckInTime = DateTime.Parse(DateTime.Now.ToString("g"));

                /*Verification random number generator*/
                //var ran = new Random();
                //while (true){
                //        do
                //        {
                //            parkedVehicle.Verification += ran.Next(0, 9);
                //        } while (parkedVehicle.Verification.Length != 4);
                //    var vehicles = db.ParkedVehicles.Where(v => v.Verification == parkedVehicle.Verification);
                //    if (!vehicles.Any())
                //    {
                //        break;
                //    }
                //    parkedVehicle.Verification = "";
                //}
                ViewBag.Description = "Fordonet har parkerats i garaget!";
                db.ParkedVehicles.Add(parkedVehicle);
                db.SaveChanges();
                return View();
            }
            ViewBag.Warning = "Det finns redan ett fordon med samma RegNr!";
            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParkedVehicle parkedVehicle = db.ParkedVehicles.Find(id);
            if (parkedVehicle == null)
            {
                return HttpNotFound();
            }
            return View(parkedVehicle);
        }

        // POST: ParkedVehicles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        /* AmountFee is no longer editable. It will not be calculated automatically from Views > ParkedVehicles > Index, Line: 71 (Linus)*/
        public ActionResult Edit([Bind(Include = "Id,RegNr,Color,Brand,Model,WheelsAmount,VehicleType,CheckInTime")] ParkedVehicle parkedVehicle)
        {
            if (ModelState.IsValid)
            {
                db.Entry(parkedVehicle).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/Delete/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParkedVehicle parkedVehicle = db.ParkedVehicles.Find(id);
            if (parkedVehicle == null)
            {
                return HttpNotFound();
            }

            parkedVehicle.CheckOutTime = DateTime.Parse(DateTime.Now.ToString("g"));
            TimeSpan? ParkingDuration = parkedVehicle.CheckOutTime - parkedVehicle.CheckInTime;
            parkedVehicle.AmountFee = 5 * (int)Math.Ceiling(ParkingDuration?.TotalMinutes / 10 ?? 0);

            return View("Details",parkedVehicle);
        }

        // POST: ParkedVehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ParkedVehicle parkedVehicle = db.ParkedVehicles.Find(id);
            db.ParkedVehicles.Remove(parkedVehicle);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /*The SearchVehicule method. Allows you to search any vehicles with: 
         * Email, Vehicule TYpe, RegNr, CheckInTime*/
        public ActionResult SearchVehicule(string Email, VehicleType VehicleType, string RegNr, DateTime? CheckInTime)
        {
            var result = db.ParkedVehicles.Where(v => v.RegNr == RegNr);
            ViewBag.Searched = "eftersomboolfunkarej";
            if (!result.Any())
            {
                if (RegNr != "")
                {
                    ViewBag.Description = "Kunde inte hitta fordonet med RegNr: " + RegNr;
                }
                else
                {
                    ViewBag.Description = "Vänligen ange ett registreringsnummer";
                }
                return View("Index", result?.ToList());
            }

            return View("Index", result.ToList());
        }



    }
}
