// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-14-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="DeviceTracks.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// The Models namespace.
/// </summary>
namespace MusicPickerService.Models
{
    /// <summary>
    /// Class that represent the track of a device.
    /// </summary>
    public class DeviceTracks
    {
        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>The device identifier.</value>
        [Key, Column(Order = 0)]
        public int DeviceId { get; set; }
        /// <summary>
        /// Gets or sets the device.
        /// </summary>
        /// <value>The device.</value>
        public virtual Device Device { get; set; }

        /// <summary>
        /// Gets or sets the track identifier.
        /// </summary>
        /// <value>The track identifier.</value>
        [Key, Column(Order = 1)]
        [Index]
        public int TrackId { get; set; }
        /// <summary>
        /// Gets or sets the track.
        /// </summary>
        /// <value>The track.</value>
        public virtual Track Track { get; set; }

        /// <summary>
        /// Gets or sets the device track identifier.
        /// </summary>
        /// <value>The device track identifier.</value>
        public string DeviceTrackId { get; set; }
        /// <summary>
        /// Gets or sets the duration of the track.
        /// </summary>
        /// <value>The duration of the track.</value>
        public int TrackDuration { get; set; }
    }
}