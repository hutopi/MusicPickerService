using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MusicPickerService.Models;
using Hangfire;

namespace MusicPickerService.Controllers
{
    [Authorize]
    [RoutePrefix("api/Devices")]
    public class DevicesController : ApiController
    {
        private ApplicationDbContext db
        {
            get { return Request.GetOwinContext().Get<ApplicationDbContext>(); }
        }

        private ApplicationUserManager userManager
        {
            get
            {
                return Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        private ApplicationUser currentUser
        {
            get
            {
                return userManager.FindById(User.Identity.GetUserId());
            }
        }

        public List<Device> GetDevices()
        {
            IQueryable<Device> result = from device in db.Devices
                where device.OwnerId == currentUser.Id
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

            if (!IsDeviceOwner(device))
            {
                return Unauthorized();
            }

            return Ok(device);
        }

        // GET: api/Devices/name
        [ResponseType(typeof(Device))]
        public IHttpActionResult GetDevice(string name)
        {
            Device device = (from d in db.Devices
                where d.Name == name
                select d).SingleOrDefault();
            
            if (device == null)
            {
                return NotFound();
            }

            if (!IsDeviceOwner(device))
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

            if (DeviceExists(currentUser.Id, input.Name))
            {
                return BadRequest(String.Format("Device with name {0} already exists", input.Name));
            }

            DateTime now = DateTime.Now;

            Device device = new Device()
            {
                Owner = currentUser,
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

            if (!IsDeviceOwner(device))
            {
                return Unauthorized();
            }

            db.Devices.Remove(device);
            db.SaveChanges();

            return Ok(device);
        }

        [Route("{id}/Submit")]
        [HttpPost]
        public IHttpActionResult SubmitMusic(int id, List<DeviceSubmission> submissions)
        {
            Device device = db.Devices.Find(id);
            if (device == null)
            {
                return NotFound();
            }

            if (!IsDeviceOwner(device))
            {
                return Unauthorized();
            }

            BackgroundJob.Enqueue<SubmitDevice>(x => x.Submit(JobCancellationToken.Null, id, submissions));

            return Ok();
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

        private bool DeviceExists(string ownerId, string deviceName)
        {
            int count = (from device in db.Devices
                where device.OwnerId == ownerId && device.Name == deviceName
                select device).Count();

            if (count != 0)
            {
                return true;
            }

            return false;
        }

        private bool IsDeviceOwner(Device device)
        {
            if (device.Owner == currentUser)
            {
                return true;
            }
            return false;
        }
    }
}