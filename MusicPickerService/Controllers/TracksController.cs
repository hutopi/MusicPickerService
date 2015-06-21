using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MusicPickerService.Models;

namespace MusicPickerService.Controllers
{
    [Authorize]
    public class TracksController : ApiController
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

        [HttpGet]
        public Track Get(int id)
        {
            return db.Tracks.Find(id);
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
