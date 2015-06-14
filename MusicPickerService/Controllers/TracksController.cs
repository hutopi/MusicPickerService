using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using MusicPickerService.Models;

namespace MusicPickerService.Controllers
{
    public class TracksController : ApiController
    {
        private ApplicationDbContext db
        {
            get { return Request.GetOwinContext().Get<ApplicationDbContext>(); }
        }

        [HttpGet]
        public IEnumerable<Track> GetByDevice(int device)
        {
            return (from d in db.DeviceTracks
                    where d.DeviceId == device
                    select d.Track).ToList();
        }

        [HttpGet]
        public IEnumerable<Track> GetByDeviceAndAlbum(int device, int album)
        {
            return (from d in db.DeviceTracks
                where d.DeviceId == device && d.Track.AlbumId == album
                select d.Track).ToList();
        }

        [HttpGet]
        public string Get(int id)
        {
            return "value";
        }
    }
}
