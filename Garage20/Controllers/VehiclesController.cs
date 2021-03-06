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
using System.Web.UI.WebControls;

namespace Garage20.Controllers
{
    public class VehiclesController : Controller
    {
        private GarageContext db = new GarageContext();

        // GET: Vehicles
        public ActionResult Index()
        {
            //ViewBag.VehicleTypeId = new SelectList(db.VehicleType, "Id", "VehicleTypeName");
            List<SelectListItem> items = new SelectList(db.VehicleType, "Id", "VehicleTypeName").ToList();
            items.Insert(0, (new SelectListItem { Text = "[Alla]", Value = "0" }));
            ViewBag.VehicleTypeId = items;

            var vehicles = db.Vehicles.Include(v => v.Member).Include(v => v.VehicleType);
            return View(vehicles.ToList());
        }

        // GET: Vehicles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicles vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }

            vehicle.CheckOutTime = DateTime.Parse(DateTime.Now.ToString("g"));
            TimeSpan? ParkingDuration = vehicle.CheckOutTime - vehicle.CheckInTime;
            vehicle.AmountFee = 5 * (int)Math.Ceiling(ParkingDuration?.TotalMinutes / 10 ?? 0);

            return View("Details", vehicle);
        }

        public ActionResult SearchVehicle()
        {
            return View();
        }

        public ActionResult CheckOut(string Search)
        {
            var result = db.Vehicles.Where(v => v.RegNr == Search);
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

                return View("CheckOut");
            }

            return Receipt(result?.First()?.Id);
        }

        public ActionResult Receipt(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Vehicles vehicle = db.Vehicles.Find(id);
            Members member = vehicle.Member;

            if (vehicle == null)
            {
                return HttpNotFound();
            }
            db.Vehicles.Remove(vehicle);
            db.SaveChanges();

            vehicle.CheckOutTime = DateTime.Parse(DateTime.Now.ToString("g"));
            TimeSpan? ParkingDuration = vehicle.CheckOutTime - vehicle.CheckInTime;
            vehicle.AmountFee = 5 * (int)Math.Ceiling(ParkingDuration?.TotalMinutes / 10 ?? 0);
            //vehicle.
            VehicleType vType = db.VehicleType?.Where(vt => vt.Id == vehicle.VehicleTypeId).SingleOrDefault();
            vehicle.VehicleType = vType;
            Members currentMember = db.Members?.Where(m => m.Id == vehicle.MemberId).SingleOrDefault();
            vehicle.Member = currentMember;

            return View("Receipt", vehicle);
        }

        public ActionResult Search(string Search, int VehicleTypeId)
        {
           


            string regnr = Search;
            int typeId = VehicleTypeId;
            IQueryable<Vehicles> vehicle = null;

            if (regnr != "" && typeId != 0)
            {
                //båda sökkriterierna
                    vehicle =  db.Vehicles
                   .Where(v => v.RegNr.Contains(regnr) && v.VehicleTypeId == typeId);
            }
            else if (regnr != "")
            {
                vehicle = db.Vehicles
                  .Where(v => v.RegNr.Contains(regnr));
            }
            else if (typeId != 0)
            {
                vehicle = db.Vehicles
                .Where(v => v.VehicleTypeId == typeId);
            }
            else
            {
                ViewBag.Description = "Vänligen ange sökkriterie";

            }

            
            List<SelectListItem> items = new SelectList(db.VehicleType, "Id", "VehicleTypeName").ToList();
            items.Insert(0, (new SelectListItem { Text = "[Alla]", Value = "0" }));
            ViewBag.VehicleTypeId = items;

            if (vehicle == null || !vehicle.Any())
            {

                ViewBag.Description = "Sökningen returnerade inga träffar";
                return View("Index", db.Vehicles.ToList());
                //db.Vehicles.ToList()
                //return View("Index", vehicle?.ToList());
            }
            else
            {
               
                return View("Index", vehicle?.ToList());
            }



            //if (regnr != "" || typeId != "")
            //{

            //    var vehicle = db.Vehicles
            //    .Where(v => v.Email.Contains(s) || v.FirstName.Contains(s) || v.LastName.Contains(s));
           
            //}
            //else
            //{
            //    
            //}

            //return View("Index", db.Members.ToList());

        }


        // GET: Vehicles/Create
        public ActionResult Create()
        {
           // ViewBag.MemberId = new SelectList(db.Members, "Id", "FirstName");

            ViewBag.VehicleTypeId = new SelectList(db.VehicleType, "Id", "VehicleTypeName");
            if (TempData["CreatedMemberEmail"] != null)
            {
                ViewBag.MemberEmail = TempData["CreatedMemberEmail"].ToString();

            }
            else
            {
                ViewBag.MemberEmail = "";
            }
            return View();
        }

        // POST: Vehicles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MemberId,Member.Email,VehicleTypeId,RegNr,Color,Brand,Model,WheelsAmount,CheckInTime")] Vehicles vehicles, string MemberEmail)
        {
            string memberMail = MemberEmail;
            Members currentMember = db.Members?.Where(m => m.Email == memberMail).SingleOrDefault();

            var vehicle = db.Vehicles.Where(v => v.RegNr == vehicles.RegNr);

            if (ModelState.IsValid && !vehicle.Any() && currentMember != null)
            {
                //Connect member and vehicle
                vehicles.MemberId = currentMember.Id;

                /*CheckInTime is now being defined by the user's current time when the user parks a car (Linus)*/
                vehicles.CheckInTime = DateTime.Parse(DateTime.Now.ToString("g"));
                

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
                db.Vehicles.Add(vehicles);
                db.SaveChanges();
                ViewBag.VehicleTypeId = new SelectList(db.VehicleType, "Id", "VehicleTypeName");
                return View();
            }
            else if(vehicle.Any())
            {
                ViewBag.Warning = "Det finns redan ett fordon med samma RegNr!";
            }
            else
            {
                ViewBag.Warning = "Denna mailadress är inte registrerat!";
            }
            

            //if (ModelState.IsValid)
            //{
            //    db.Vehicles.Add(vehicles);
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}

            ViewBag.VehicleTypeId = new SelectList(db.VehicleType, "Id", "VehicleTypeName", vehicles.VehicleTypeId);
            return View(vehicles);
        }

        // GET: Vehicles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicles vehicles = db.Vehicles.Find(id);
            if (vehicles == null)
            {
                return HttpNotFound();
            }
            
            ViewBag.MemberId = new SelectList(db.Members, "Id", "FirstName", vehicles.MemberId);
            ViewBag.VehicleTypeId = new SelectList(db.VehicleType, "Id", "VehicleTypeName", vehicles.VehicleTypeId);
            return View(vehicles);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,MemberId,VehicleTypeId,Verification,RegNr,Color,Brand,Model,WheelsAmount,CheckInTime,CheckOutTime,AmountFee")] Vehicles vehicles)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vehicles).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MemberId = new SelectList(db.Members, "Id", "FirstName", vehicles.MemberId);
            ViewBag.VehicleTypeId = new SelectList(db.VehicleType, "Id", "VehicleTypeName", vehicles.VehicleTypeId);
            return View(vehicles);
        }

        // GET: Vehicles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicles vehicles = db.Vehicles.Find(id);
            if (vehicles == null)
            {
                return HttpNotFound();
            }
            return View(vehicles);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vehicles vehicles = db.Vehicles.Find(id);
            db.Vehicles.Remove(vehicles);
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
    }
}
