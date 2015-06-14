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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MusicPickerService.Models;

namespace MusicPickerService.Controllers
{
    [Authorize]
    public class DevicesController : ApiController
    {
        private ApplicationDbContext db
        {
            get { return Request.GetOwinContext().Get<ApplicationDbContext>(); }
        }

        private ApplicationUserManager UserManager
        {
            get
            {
                return Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        private ApplicationUser CurrentUser
        {
            get
            {
                return UserManager.FindById(User.Identity.GetUserId());
            }
        }

        public List<Device> GetDevices()
        {
            IQueryable<Device> result = from device in db.Devices
                where device.OwnerId == CurrentUser.Id
                select device;
            List<Device> r = result.ToList();

            return r;
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

            if (!isDeviceOwner(device))
            {
                return Unauthorized();
            }

            return Ok(device);
        }

        // POST: api/Devices
        [ResponseType(typeof(Device))]
        public IHttpActionResult PostDevice(DeviceBindingModel input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DateTime now = DateTime.Now;

            Device device = new Device()
            {
                Owner = CurrentUser,
                Name = input.Name,
                AccessDate = now,
                RegistrationDate = now
            };

            db.Devices.Add(device);
            db.SaveChanges();

            return Ok(device);
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

            if (!isDeviceOwner(device))
            {
                return Unauthorized();
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

        private bool isDeviceOwner(Device device)
        {
            if (device.Owner == CurrentUser)
            {
                return true;
            }
            return false;
        }
    }
}