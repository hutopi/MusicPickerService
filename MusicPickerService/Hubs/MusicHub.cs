using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MusicPickerService.Models;
using StackExchange.Redis;

namespace MusicPickerService.Hubs
{
    public class MusicHub
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
        }

        public void Play(int deviceId)
        {
            
        }

        public void Pause(int deviceId)
        {
            
        }

        public void Stop(int deviceId)
        {
            
        }
    }
}