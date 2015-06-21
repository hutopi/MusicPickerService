// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-21-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="Genre.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.ComponentModel.DataAnnotations;

/// <summary>
/// The Models namespace.
/// </summary>
namespace MusicPickerService.Models
{
    /// <summary>
    /// Class that represents a music style.
    /// </summary>
    public class Genre
    {
        /// <summary>
        /// Gets or sets the identifier of the genre.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the genre.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
    }
}
