﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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

        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        [Authorize]
        public void RegisterDevice(int deviceId)
        {
            Device device = dbContext.Devices.Find(deviceId);
            if (device.OwnerId != Context.User.Identity.GetUserId())
            {
                return;
            }

            Store.KeyDelete(String.Format("musichub.device.{0}.current", deviceId));
            Store.KeyDelete(String.Format("musichub.device.{0}.duration", deviceId));
            Store.KeyDelete(String.Format("musichub.device.{0}.playing", deviceId));
            Store.KeyDelete(String.Format("musichub.device.{0}.lastpause", deviceId));
            Store.KeyDelete(String.Format("musichub.device.{0}.queue", deviceId));
            Store.KeyDelete(String.Format("musichub.device.{0}.queue.device", deviceId));
            Store.KeyDelete(String.Format("musichub.device.{0}.clients", deviceId));
            Store.StringSet(String.Format("musichub.device.{0}.connection", deviceId), Context.ConnectionId);
        }

        public void RegisterClient(int deviceId)
        {
            Device device = dbContext.Devices.Find(deviceId);
            if (device.OwnerId != Context.User.Identity.GetUserId())
            {
                return;
            }

            Groups.Add(Context.ConnectionId, String.Format("device.{0}", deviceId));
            Store.SetAdd(String.Format("musichub.device.{0}.clients", deviceId), Context.ConnectionId);
        }

        private DeviceState CreateDeviceState(int deviceId)
        {
            return new DeviceState()
            {
                Current = (int)Store.StringGet(String.Format("musichub.device.{0}.current", deviceId)),
                Duration = (int)Store.StringGet(String.Format("musichub.device.{0}.duration", deviceId)),
                Playing = (bool)Store.StringGet(String.Format("musichub.device.{0}.playing", deviceId)),
                LastPause = DateTime.FromFileTimeUtc((long)Store.StringGet(String.Format("musichub.device.{0}.lastpause", deviceId))),
                Queue = (int[])Array.ConvertAll(Store.ListRange(String.Format("musichub.device.{0}.queue", deviceId)), item => (int)item)
            };
        }
        private void SendClientState(int deviceId)
        {
            Clients.Group(String.Format("device.{0}", deviceId)).SetState(CreateDeviceState(deviceId));
        }

        private bool IsRegistered(int deviceId)
        {
            if (Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId)) == Context.ConnectionId)
            {
                return true;
            }

            return Store.SetContains(String.Format("musichub.device.{0}.clients", deviceId), Context.ConnectionId);
        }

        public void Queue(int deviceId, int[] trackIds)
        {
            if (!IsRegistered(deviceId))
            {
                return;
            }

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

            int current = (int)Store.ListLeftPop(String.Format("musichub.device.{0}.queue", deviceId), 0);
            string currentDeviceTrack = Store.ListLeftPop(String.Format("musichub.device.{0}.queue.device", deviceId), 0);

            string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Stop();
            Clients.Client(deviceClientId).SetTrackId(currentDeviceTrack);
        }

        public void GetState(int deviceId)
        {
            if (!IsRegistered(deviceId))
            {
                return;
            }

            Clients.Caller.SetState(CreateDeviceState(deviceId));
        }

        public void Play(int deviceId)
        {
            if (!IsRegistered(deviceId))
            {
                return;
            }

            Store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), true);
            Store.StringSet(String.Format("musichub.device.{0}.lastpause", deviceId), DateTime.Now.ToFileTimeUtc());

            SendClientState(deviceId);

            string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Play();
        }

        public void Pause(int deviceId)
        {
            if (!IsRegistered(deviceId))
            {
                return;
            }

            Store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), false);
            Store.StringSet(String.Format("musichub.device.{0}.lastpause", deviceId), DateTime.Now.ToFileTimeUtc());

            SendClientState(deviceId);

            string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Pause();
        }

        public void RequestNext(int deviceId)
        {
            if (!IsRegistered(deviceId))
            {
                return;
            }

            Store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), false);
            Store.KeyDelete(String.Format("musichub.device.{0}.lastpause", deviceId));

            SendClientState(deviceId);

            string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Stop();
        }

        public void Next(int deviceId)
        {
            if (!IsRegistered(deviceId))
            {
                return;
            }

            if (Store.ListLength(String.Format("musichub.device.{0}.queue", deviceId)) == 0)
            {
                return;
            }

            int current = (int) Store.ListLeftPop(String.Format("musichub.device.{0}.queue", deviceId), 0);
            string currentDeviceTrack = Store.ListLeftPop(String.Format("musichub.device.{0}.queue.device", deviceId), 0);
            Store.StringSet(String.Format("musichub.device.{0}.current", deviceId), current);
            
            int duration = (from dt in this.dbContext.DeviceTracks
                                    where dt.DeviceId == deviceId && dt.TrackId == current
                                    select dt.TrackDuration).First();
            Store.StringSet(String.Format("musichub.device.{0}.duration", deviceId), duration);

            SendClientState(deviceId);

            if (currentDeviceTrack != null)
            {
                string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
                Clients.Client(deviceClientId).SetTrackId(currentDeviceTrack);

                Play(deviceId);
            }
        }
    }
}