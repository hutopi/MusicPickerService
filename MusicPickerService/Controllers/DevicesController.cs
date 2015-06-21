// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-12-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="DevicesController.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
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

/// <summary>
/// The Controllers namespace.
/// </summary>
namespace MusicPickerService.Controllers
{
    /// <summary>
    /// Class DevicesController.
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Devices")]
    public class DevicesController : ApiController
    {
        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>The database.</value>
        private ApplicationDbContext db
        {
            get { return Request.GetOwinContext().Get<ApplicationDbContext>(); }
        }

        /// <summary>
        /// Gets the user manager.
        /// </summary>
        /// <value>The user manager.</value>
        private ApplicationUserManager userManager
        {
            get
            {
                return Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        private ApplicationUser currentUser
        {
            get
            {
                return userManager.FindById(User.Identity.GetUserId());
            }
        }

        /// <summary>
        /// Gets the all devices.
        /// </summary>
        /// <returns>List&lt;Device&gt;.</returns>
        public List<Device> GetDevices()
        {
            IQueryable<Device> result = from device in db.Devices
                                        where device.OwnerId == currentUser.Id
                                        select device;
            List<Device> r = result.ToList();

            return r;
        }

        // GET: api/Devices/5
        /// <summary>
        /// Gets the device by its identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>IHttpActionResult.</returns>
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
        /// <summary>
        /// Gets the device by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IHttpActionResult.</returns>
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
        /// <summary>
        /// Posts the device.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>IHttpActionResult.</returns>
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
        /// <summary>
        /// Deletes the device.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>IHttpActionResult.</returns>
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

        /// <summary>
        /// Submit the music.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="submissions">The submissions.</param>
        /// <returns>IHttpActionResult.</returns>
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

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and, optionally, releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Return if the device exist or not by its identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool DeviceExists(int id)
        {
            return db.Devices.Count(e => e.Id == id) > 0;
        }

        /// <summary>
        /// Return if the device exist or not by its name and owner identifier.
        /// </summary>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="deviceName">Name of the device.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
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

        /// <summary>
        /// Determines whether [is device owner] [the specified device].
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns><c>true</c> if [is device owner] [the specified device]; otherwise, <c>false</c>.</returns>
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