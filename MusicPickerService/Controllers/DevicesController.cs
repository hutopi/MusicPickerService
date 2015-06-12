using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using MusicPickerService.Models;

namespace MusicPickerService.Controllers
{
    public class DevicesController : ApiController
    {
        private Context db = new Context();

        // GET: api/Devices
        public IQueryable<Device> GetDevices()
        {
            return db.Devices;
        }

        // GET: api/Devices/5
        [ResponseType(typeof(Device))]
        public IHttpActionResult GetDevice(int id)
        {
            Device device = db.Devices.Find(id);
            if (device == null)
            {
                return NotFound();
            }

            return Ok(device);
        }

        // PUT: api/Devices/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutDevice(int id, Device device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != device.Id)
            {
                return BadRequest();
            }

            db.Entry(device).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Devices
        [ResponseType(typeof(Device))]
        public IHttpActionResult PostDevice(Device device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Devices.Add(device);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = device.Id }, device);
        }

        // DELETE: api/Devices/5
        [ResponseType(typeof(Device))]
        public IHttpActionResult DeleteDevice(int id)
        {
            Device device = db.Devices.Find(id);
            if (device == null)
            {
                return NotFound();
            }

            db.Devices.Remove(device);
            db.SaveChanges();

            return Ok(device);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DeviceExists(int id)
        {
            return db.Devices.Count(e => e.Id == id) > 0;
        }
    }
}