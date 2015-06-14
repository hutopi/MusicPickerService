using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MusicPickerService.Models;
using Owin;

namespace MusicPickerService
{
    public partial class Startup
    {
        public void ConfigureDbContext(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
        }
    }
}