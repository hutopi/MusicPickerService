// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-14-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="ApplicationDbContext.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

/// <summary>
/// The Models namespace.
/// </summary>
namespace MusicPickerService.Models
{
    /// <summary>
    /// Class that represents the Db context of the application.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>ApplicationDbContext.</returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        /// <summary>
        /// Gets or sets the devices.
        /// </summary>
        /// <value>The devices.</value>
        public DbSet<Device> Devices { get; set; }
        /// <summary>
        /// Gets or sets the artists.
        /// </summary>
        /// <value>The artists.</value>
        public DbSet<Artist> Artists { get; set; }
        /// <summary>
        /// Gets or sets the albums.
        /// </summary>
        /// <value>The albums.</value>
        public DbSet<Album> Albums { get; set; }
        /// <summary>
        /// Gets or sets the tracks.
        /// </summary>
        /// <value>The tracks.</value>
        public DbSet<Track> Tracks { get; set; }
        /// <summary>
        /// Gets or sets the genres.
        /// </summary>
        /// <value>The genres.</value>
        public DbSet<Genre> Genres { get; set; }
        /// <summary>
        /// Gets or sets the device tracks.
        /// </summary>
        /// <value>The device tracks.</value>
        public DbSet<DeviceTracks> DeviceTracks { get; set; }
    }
}