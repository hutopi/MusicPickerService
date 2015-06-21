// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-12-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="Startup.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.Owin;
using Owin;

/// <summary>
/// The MusicPickerService namespace.
/// </summary>
[assembly: OwinStartup(typeof(MusicPickerService.Startup))]

namespace MusicPickerService
{
    /// <summary>
    /// Class Startup.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configurations the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureHangfire(app);
            ConfigureDbContext(app);
            ConfigureAuth(app);
            ConfigureSignalr(app);
        }
    }
}
