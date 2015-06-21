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