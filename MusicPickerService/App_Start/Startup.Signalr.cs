using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;

namespace MusicPickerService
{
    public partial class Startup
    {
        public void ConfigureSignalr(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}