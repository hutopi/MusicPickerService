using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MusicPickerService.Models;
using StackExchange.Redis;
using Microsoft.AspNet.SignalR;

namespace MusicPickerService.Hubs
{
    public class MusicHub: Hub
    {
        private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        private IDatabase Store
        {
            get
            {
                return redis.GetDatabase();
            }
        }

        public void Queue(int deviceId, int[] trackIds)
        {
            string queue = String.Format("musichub.device.{0}.queue", deviceId);
            string deviceQueue = String.Format("musichub.device.{0}.queue.device", deviceId);
            Store.KeyDelete(queue);
            Store.KeyDelete(deviceQueue);

            foreach (int trackId in trackIds)
            {
                Store.ListRightPush(queue, trackId);
                string trackDeviceId = (from dt in this.dbContext.DeviceTracks
                    where dt.DeviceId == deviceId && dt.TrackId == trackId
                    select dt.DeviceTrackId).First();
                Store.ListRightPush(deviceQueue, trackDeviceId);
            }

            SendClientState(deviceId);
        }

        public void RegisterDevice(int deviceId)
        {
            Store.KeyDelete(String.Format("musichub.device.{0}.current", deviceId));
            Store.KeyDelete(String.Format("musichub.device.{0}.playing", deviceId));
            Store.KeyDelete(String.Format("musichub.device.{0}.lastpause", deviceId));
            Store.KeyDelete(String.Format("musichub.device.{0}.queue", deviceId));
            Store.KeyDelete(String.Format("musichub.device.{0}.queue.device", deviceId));
            Store.StringSet(String.Format("musichub.device.{0}.connection", deviceId), Context.ConnectionId);
        }

        private DeviceState CreateDeviceState(int deviceId)
        {
            return new DeviceState()
            {
                Current = (int) Store.StringGet(String.Format("musichub.device.{0}.current", deviceId)),
                Playing = (bool) Store.StringGet(String.Format("musichub.device.{0}.playing", deviceId)),
                LastPause = DateTime.FromFileTimeUtc((long) Store.StringGet(String.Format("musichub.device.{0}.lastpause", deviceId))),
                Queue = (int[]) Array.ConvertAll(Store.ListRange(String.Format("musichub.device.{0}.queue", deviceId)), item => (int) item)
            };
        }

        public void SendClientState(int deviceId)
        {
            Clients.Group(String.Format("device.{0}", deviceId)).SetState(CreateDeviceState(deviceId));
        }

        // Called by app to request a device to control
        public void ConnectToDevice(int deviceId)
        {
            Groups.Add(Context.ConnectionId, String.Format("device.{0}", deviceId));
        }

        public void Play(int deviceId)
        {
            Store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), true);
            Store.StringSet(String.Format("musichub.device.{0}.lastpause", deviceId), DateTime.Now.ToFileTimeUtc());

            SendClientState(deviceId);

            string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Play();
        }

        public void Pause(int deviceId)
        {
            Store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), false);
            Store.StringSet(String.Format("musichub.device.{0}.lastpause", deviceId), DateTime.Now.ToFileTimeUtc());

            SendClientState(deviceId);

            string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Pause();
        }

        public void Next(int deviceId)
        {
            Store.ListLeftPop(String.Format("musichub.device.{0}.queue", deviceId), 0);
            int current = (int)Store.ListLeftPop(String.Format("musichub.device.{0}.queue.device", deviceId), 0);
            Store.StringSet(String.Format("musichub.device.{0}.current", deviceId), current);
            
            SendClientState(deviceId);

            string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).SetTrackId(current);
        }

        public void GetState(int deviceId)
        {
            Clients.Caller.SetState(CreateDeviceState(deviceId));
        }
    }
}