// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-14-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="TracksController.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MusicPickerService.Models;

/// <summary>
/// The Controllers namespace.
/// </summary>
namespace MusicPickerService.Controllers
{
    /// <summary>
    /// Class TracksController.
    /// </summary>
    [Authorize]
    public class TracksController : ApiController
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
        /// Gets the tracks by device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns>IHttpActionResult.</returns>
        [HttpGet]
        public IHttpActionResult GetByDevice(int device)
        {
            Device currentDevice = db.Devices.Find(device);
            if (currentDevice == null)
            {
                return NotFound();
            }

            if (!IsDeviceOwner(currentDevice))
            {
                return Unauthorized();
            }

            return Ok((from d in db.DeviceTracks
                       where d.DeviceId == device
                       select d.Track).ToList());
        }

        /// <summary>
        /// Gets the tracks by device and which belong to the album identifier.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="album">The album.</param>
        /// <returns>IHttpActionResult.</returns>
        [HttpGet]
        public IHttpActionResult GetByDeviceAndAlbum(int device, int album)
        {
            Device currentDevice = db.Devices.Find(device);
            if (currentDevice == null)
            {
                return NotFound();
            }

            if (!IsDeviceOwner(currentDevice))
            {
                return Unauthorized();
            }

            return Ok((from d in db.DeviceTracks
                       where d.DeviceId == device && d.Track.AlbumId == album
                       select d.Track).ToList());
        }

        /// <summary>
        /// Gets the track which have id as identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Track.</returns>
        [HttpGet]
        public Track Get(int id)
        {
            return db.Tracks.Find(id);
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
