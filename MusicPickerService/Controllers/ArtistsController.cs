using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MusicPickerService.Models;

namespace MusicPickerService.Controllers
{
    [Authorize]
    public class ArtistsController : ApiController
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

        [HttpGet]
        public IHttpActionResult GetByDevice(int device)
        {
            Device currentDevice = db.Devices.Find(device);
            if (currentDevice == null)
            {
                return NotFound();
            }

            if (!isDeviceOwner(currentDevice))
            {
                return Unauthorized();
            }

            return Ok((from d in db.DeviceTracks
                    where d.DeviceId == device
                    select d.Track.Album.Artist).Distinct().ToList());
        }

        [HttpGet]
        public Artist Get(int id)
        {
            return db.Artists.Find(id);
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
