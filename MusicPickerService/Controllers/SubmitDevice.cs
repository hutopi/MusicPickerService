﻿// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-19-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="SubmitDevice.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net.Http;
using Hangfire;
using MusicPickerService.Models;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Controllers namespace.
/// </summary>
namespace MusicPickerService.Controllers
{
    /// <summary>
    /// Class SubmitDevice.
    /// </summary>
    public class SubmitDevice
    {
        /// <summary>
        /// The database
        /// </summary>
        private ApplicationDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitDevice"/> class.
        /// </summary>
        public SubmitDevice()
        {
            this.db = new ApplicationDbContext();
        }

        /// <summary>
        /// Gets the artwork of an album.
        /// </summary>
        /// <param name="submission">The submission.</param>
        public void GetArtwork(DeviceSubmission submission)
        {
            string key = "2c2e6ce34b0d78dac557611b898bf547";
            Uri uri = new Uri(String.Format("http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key={0}&artist={1}&album={2}&format=json", key, submission.Artist, submission.Album));
            try
            {
                HttpResponseMessage result = (new HttpClient()).GetAsync(uri).Result;

                if (result.IsSuccessStatusCode)
                {
                    JObject search = JObject.Parse(result.Content.ReadAsStringAsync().Result);
                    if (search["album"] != null)
                    {
                        if (search["album"]["image"] != null)
                        {
                            List<JToken> results = search["album"]["image"].Children().ToList();
                            if (results.Count > 0)
                            {
                                string url = results[results.Count - 1]["#text"].ToString();

                                Album album = (from a in db.Albums
                                               where a.Name == submission.Album
                                               select a).First();

                                album.Artwork = url;
                                db.Albums.AddOrUpdate(album);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Submits the specified submissions.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="submissions">The submissions.</param>
        public void Submit(IJobCancellationToken cancellationToken, int id, List<DeviceSubmission> submissions)
        {
            db.DeviceTracks.RemoveRange(db.DeviceTracks.Where(e => e.DeviceId == id));
            db.SaveChanges();

            foreach (DeviceSubmission submission in submissions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Artist artist;
                if (submission.Artist == null)
                {
                    submission.Artist = "Unknown artist";
                }
                if (submission.Album == null)
                {
                    submission.Album = "Unknown album";
                }
                if (submission.Title == null)
                {
                    if (submission.Count != 0)
                    {
                        submission.Title = String.Format("Track {0}", submission.Count);
                    }
                    else
                    {
                        submission.Title = "Unknown track";
                    }
                }

                if (db.Artists.Count(a => a.Name == submission.Artist) == 0)
                {
                    artist = new Artist() { Name = submission.Artist };
                    db.Artists.Add(artist);
                }
                else
                {
                    artist = (from a in db.Artists
                              where a.Name == submission.Artist
                              select a).First();
                }

                Album album;
                if (db.Albums.Count(a => a.Name == submission.Album && a.ArtistId == artist.Id) == 0)
                {
                    album = new Album() { Name = submission.Album, Year = submission.Year, ArtistId = artist.Id };
                    db.Albums.Add(album);

                    BackgroundJob.Enqueue<SubmitDevice>(x => x.GetArtwork(submission));
                }
                else
                {
                    album = (from a in db.Albums
                             where a.Name == submission.Album && a.ArtistId == artist.Id
                             select a).First();
                }

                Track track;
                if (db.Tracks.Count(t => t.Name == submission.Title && t.AlbumId == album.Id) == 0)
                {
                    track = new Track()
                    {
                        Name = submission.Title,
                        Number = submission.Number,
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

                if (db.DeviceTracks.Count(dt => dt.DeviceId == id && dt.TrackId == track.Id) == 0)
                {
                    DeviceTracks deviceTrack = new DeviceTracks()
                    {
                        DeviceId = id,
                        TrackId = track.Id,
                        DeviceTrackId = submission.Id,
                        TrackDuration = submission.Duration
                    };
                    db.DeviceTracks.Add(deviceTrack);
                }

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {

                }
            }
        }
    }

}
