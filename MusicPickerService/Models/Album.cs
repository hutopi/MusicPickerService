// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-21-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="Album.cs" company="Hutopi">
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
    /// Class that represents an album.
    /// </summary>
    [DataContract]
    public class Album
    {
        /// <summary>
        /// Gets or sets the identifier of the album.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the album.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        [DataMember]
        [Index("IX_NameAndArtist", 1)]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the year of the album.
        /// </summary>
        /// <value>The year.</value>
        [DataMember]
        public int Year { get; set; }
        /// <summary>
        /// Gets or sets the mb identifier of the album (MusicBrainz).
        /// </summary>
        /// <value>The mb identifier.</value>
        [DataMember]
        public string MbId { get; set; }

        /// <summary>
        /// Gets or sets the artist identifier of the album.
        /// </summary>
        /// <value>The artist identifier.</value>
        [Required]
        [DataMember]
        [Index("IX_NameAndArtist", 2)]
        public int ArtistId { get; set; }
        /// <summary>
        /// Gets or sets the artist of the album.
        /// </summary>
        /// <value>The artist.</value>
        public virtual Artist Artist { get; set; }

        /// <summary>
        /// Gets or sets the artwork of the album.
        /// </summary>
        /// <value>The artwork.</value>
        [DataMember]
        public string Artwork { get; set; }

        /// <summary>
        /// Gets or sets the tracks of the album.
        /// </summary>
        /// <value>The tracks.</value>
        public virtual ICollection<Track> Tracks { get; set; }
    }
}
