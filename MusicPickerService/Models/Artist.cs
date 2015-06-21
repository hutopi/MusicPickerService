// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-21-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="Artist.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

/// <summary>
/// The Models namespace.
/// </summary>
namespace MusicPickerService.Models
{
    /// <summary>
    /// Class that represents an artist.
    /// </summary>
    [DataContract]
    public class Artist
    {
        /// <summary>
        /// Gets or sets the identifier of the artist.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the artist.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        [DataMember]
        [Index]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the mb identifier of the artist (MusicBrainz).
        /// </summary>
        /// <value>The mb identifier.</value>
        [DataMember]
        public string MbId { get; set; }

        /// <summary>
        /// Gets or sets the albums of the artist.
        /// </summary>
        /// <value>The albums.</value>
        public virtual ICollection<Album> Albums { get; set; }
    }
}
