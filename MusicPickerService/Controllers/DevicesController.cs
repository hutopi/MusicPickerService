﻿using System;
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
    [RoutePrefix("api/Devices")]
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

            if (DeviceExists(CurrentUser.Id, input.Name))
            {
                return BadRequest(String.Format("Device with name {0} already exists", input.Name));
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

        [Route("{id}/Submit")]
        [HttpPost]
        public IHttpActionResult SubmitMusic(int id, List<DeviceSubmission> submissions)
        {
            foreach (DeviceSubmission submission in submissions)
            {
                Artist artist;
                if (db.Artists.Count(a => a.Name == submission.Artist) == 0)
                {
                    artist = new Artist() {Name = submission.Artist};
                    db.Artists.Add(artist);
                }
                else
                {
                    artist = (from a in db.Artists
                        where a.Name == submission.Artist
                        select a).First();
                }
                db.SaveChanges();

                Album album;
                if (db.Albums.Count(a => a.Name == submission.Album && a.ArtistId == artist.Id) == 0)
                {
                    album = new Album() { Name = submission.Album, Year = submission.Year, ArtistId = artist.Id};
                    db.Albums.Add(album);
                }
                else
                {
                    album = (from a in db.Albums
                              where a.Name == submission.Album && a.ArtistId == artist.Id
                              select a).First();
                }
                db.SaveChanges();

                Track track;
                if (db.Tracks.Count(t => t.Name == submission.Title && t.AlbumId == album.Id) == 0)
                {
                    track = new Track()
                    {
                        Name = submission.Title, 
                        Number = (int) submission.Number,
                        AlbumId = album.Id
                    };
                    db.Tracks.Add(track);
                }
                else
                {
                    track = (from t in db.Tracks
                             where t.Name == submission.Title && t.AlbumId == album.Id
                             select t).First();
                }
                db.SaveChanges();

                if (db.DeviceTracks.Count(dt => dt.DeviceId == id && dt.TrackId == track.Id) == 0)
                {
                    DeviceTracks deviceTrack = new DeviceTracks()
                    {
                        DeviceId = id,
                        TrackId = track.Id
                    };
                    db.DeviceTracks.Add(deviceTrack);
                    db.SaveChanges();
                }
            }

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