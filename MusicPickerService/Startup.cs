using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MusicPickerService.Startup))]

namespace MusicPickerService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureDbContext(app);
            ConfigureSignalr(app);
            ConfigureAuth(app);
        }
    }
}
