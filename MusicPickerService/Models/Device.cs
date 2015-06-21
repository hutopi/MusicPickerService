// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-21-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="Device.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

/// <summary>
/// The Models namespace.
/// </summary>
namespace MusicPickerService.Models
{
    /// <summary>
    /// Class that represents a device.
    /// </summary>
    [DataContract]
    public class Device
    {
        /// <summary>
        /// Gets or sets the identifier of the device.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the owner identifier of the device.
        /// </summary>
        /// <value>The owner identifier.</value>
        public string OwnerId { get; set; }
        /// <summary>
        /// Gets or sets the owner of the device.
        /// </summary>
        /// <value>The owner.</value>
        public virtual ApplicationUser Owner { get; set; }

        /// <summary>
        /// Gets or sets the registration date of the device.
        /// </summary>
        /// <value>The registration date.</value>
        [DataMember]
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Gets or sets the last access date of the device.
        /// </summary>
        /// <value>The access date.</value>
        [DataMember]
        public DateTime AccessDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the device.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the device tracks of the device.
        /// </summary>
        /// <value>The device tracks.</value>
        public virtual ICollection<DeviceTracks> DeviceTracks { get; set; }
    }
}
