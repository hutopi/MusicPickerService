// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-21-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="Track.cs" company="Hutopi">
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
    /// Class that represents a track.
    /// </summary>
    [DataContract]
    public class Track
    {
        /// <summary>
        /// Gets or sets the identifier of the track.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the track.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        [DataMember]
        [Index("IX_NameAndAlbum", 1)]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number of the track if it belongs to an album.
        /// </summary>
        /// <value>The number.</value>
        [DataMember]
        public int Number { get; set; }
        /// <summary>
        /// Gets or sets the mb identifier of the track (MusicBrainz).
        /// </summary>
        /// <value>The mb identifier.</value>
        [DataMember]
        public string MbId { get; set; }

        /// <summary>
        /// Gets or sets the album identifier of the track.
        /// </summary>
        /// <value>The album identifier.</value>
        [Required]
        [DataMember]
        [Index("IX_NameAndAlbum", 2)]
        public int AlbumId { get; set; }
        /// <summary>
        /// Gets or sets the album of the track.
        /// </summary>
        /// <value>The album.</value>
        public virtual Album Album { get; set; }

        /// <summary>
        /// Gets or sets the genre identifier of the track.
        /// </summary>
        /// <value>The genre identifier.</value>
        public int? GenreId { get; set; }
        /// <summary>
        /// Gets or sets the genre of the track.
        /// </summary>
        /// <value>The genre.</value>
        public virtual Genre Genre { get; set; }

        /// <summary>
        /// Gets or sets the device tracks.
        /// </summary>
        /// <value>The device tracks.</value>
        public virtual ICollection<DeviceTracks> DeviceTracks { get; set; }
    }
}
