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
    public class ArtistsController : ApiController
    {
        private ApplicationDbContext db
        {
            get { return Request.GetOwinContext().Get<ApplicationDbContext>(); }
        }

        [HttpGet]
        public IEnumerable<Artist> GetByDevice(int device)
        {
            return (from d in db.DeviceTracks
                    where d.DeviceId == device
                    select d.Track.Album.Artist).Distinct().ToList();
        }

        [HttpGet]
        public string Get(int id)
        {
            return "value";
        }
    }
}
