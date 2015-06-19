using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Hangfire;
using System.Messaging;

namespace MusicPickerService
{
    public partial class Startup
    {
        public void ConfigureHangfire(IAppBuilder app)
        {
            if (!MessageQueue.Exists(@".\private$\musicpicker"))
            {
                MessageQueue.Create(@".\private$\musicpicker", true);
            }

            GlobalConfiguration.Configuration.
                UseSqlServerStorage("DefaultConnection").
                UseMsmqQueues(@".\private$\musicpicker");

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}