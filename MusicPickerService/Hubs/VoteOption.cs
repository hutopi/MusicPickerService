// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-20-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-20-2015
// ***********************************************************************
// <copyright file="VoteOption.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
/// <summary>
/// The Hubs namespace.
/// </summary>
namespace MusicPickerService.Hubs
{
    /// <summary>
    /// Class that represents a poll option.
    /// </summary>
    public class VoteOption
    {
        /// <summary>
        /// Gets or sets the track identifier.
        /// </summary>
        /// <value>The track identifier.</value>
        public int TrackId { get; set; }
        /// <summary>
        /// Gets or sets the number of votes for the TrackId.
        /// </summary>
        /// <value>The votes.</value>
        public int Votes { get; set; }
    }
}