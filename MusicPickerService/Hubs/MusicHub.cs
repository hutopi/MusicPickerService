﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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

            Store.StringSet(String.Format("musichub.devices.{0}", Context.ConnectionId), deviceId);
            Store.StringSet(String.Format("musichub.device.{0}.connection", deviceId), Context.ConnectionId);
        }

        [Authorize]
        public void RegisterClient(int deviceId)
        {
            Device device = dbContext.Devices.Find(deviceId);
            if (device.OwnerId != Context.User.Identity.GetUserId())
            {
                return;
            }

            Store.SetAdd(String.Format("musichub.client.{0}.devices", Context.ConnectionId), deviceId);
            Groups.Add(Context.ConnectionId, String.Format("device.{0}", deviceId));
            Store.SetAdd(String.Format("musichub.device.{0}.clients", deviceId), Context.ConnectionId);
        }

        public bool DeviceConnected(int deviceId)
        {
            return Store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId));
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string deviceId;
            deviceId = Store.StringGet(String.Format("musichub.devices.{0}", Context.ConnectionId));

            if (deviceId != null)
            {
                Store.KeyDelete(String.Format("musichub.device.{0}.current", deviceId));
                Store.KeyDelete(String.Format("musichub.device.{0}.duration", deviceId));
                Store.KeyDelete(String.Format("musichub.device.{0}.playing", deviceId));
                Store.KeyDelete(String.Format("musichub.device.{0}.lastpause", deviceId));
                Store.KeyDelete(String.Format("musichub.device.{0}.queue", deviceId));
                Store.KeyDelete(String.Format("musichub.device.{0}.queue.device", deviceId));
                Store.KeyDelete(String.Format("musichub.device.{0}.clients", deviceId));
                Store.KeyDelete(String.Format("musichub.device.{0}.connection", deviceId));
            }
            else
            {
                RedisValue[] members = Store.SetMembers(String.Format("musichub.client.{0}.devices", Context.ConnectionId));
                foreach (string member in members)
                {
                    Store.SetRemove(String.Format("musichub.client.{0}.devices", Context.ConnectionId), member);
                    Groups.Remove(Context.ConnectionId, String.Format("device.{0}", member));
                    Store.SetRemove(String.Format("musichub.device.{0}.clients", member), Context.ConnectionId);
                }
            }

            return base.OnDisconnected(stopCalled);
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
            if (!IsRegistered(deviceId) 
                || 
                !Store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            string queue = String.Format("musichub.device.{0}.queue", deviceId);
            string deviceQueue = String.Format("musichub.device.{0}.queue.device", deviceId);
            Store.KeyDelete(queue);
            Store.KeyDelete(deviceQueue);

            foreach (int trackId in trackIds)
            {
                try
                {
                    string trackDeviceId = (from dt in this.dbContext.DeviceTracks
                        where dt.DeviceId == deviceId && dt.TrackId == trackId
                        select dt.DeviceTrackId).First();
                    Store.ListRightPush(queue, trackId);
                    Store.ListRightPush(deviceQueue, trackDeviceId);
                }
                catch
                {
                    continue;
                }
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
            if (!IsRegistered(deviceId)
                ||
                !Store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            Clients.Caller.SetState(CreateDeviceState(deviceId));
        }

        public void Play(int deviceId)
        {
            if (!IsRegistered(deviceId)
                ||
                !Store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            bool sendTrack = (int) Store.ListLength(String.Format("musichub.device.{0}.queue", deviceId)) == 0;
            QueueVote(deviceId);

            Store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), true);
            Store.StringSet(String.Format("musichub.device.{0}.lastpause", deviceId), DateTime.Now.ToFileTimeUtc());

            string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));

            if (sendTrack && Store.ListLength(String.Format("musichub.device.{0}.queue", deviceId)) > 0)
            {
                Clients.Client(deviceClientId).Stop();
                string currentDeviceTrack = UpdateState(deviceId);
                Clients.Client(deviceClientId).SetTrackId(currentDeviceTrack);
            }
            Clients.Client(deviceClientId).Play();
            SendClientState(deviceId);
        }

        public void Pause(int deviceId)
        {
            if (!IsRegistered(deviceId)
                ||
                !Store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
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
            if (!IsRegistered(deviceId)
                ||
                !Store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            Store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), false);
            Store.KeyDelete(String.Format("musichub.device.{0}.lastpause", deviceId));

            SendClientState(deviceId);

            string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Stop();
        }

        private string UpdateState(int deviceId)
        {
            int current = (int) Store.ListLeftPop(String.Format("musichub.device.{0}.queue", deviceId), 0);
            string currentDeviceTrack = Store.ListLeftPop(String.Format("musichub.device.{0}.queue.device", deviceId), 0);
            Store.StringSet(String.Format("musichub.device.{0}.current", deviceId), current);
            
            int duration = (from dt in this.dbContext.DeviceTracks
                                    where dt.DeviceId == deviceId && dt.TrackId == current
                                    select dt.TrackDuration).First();
            Store.StringSet(String.Format("musichub.device.{0}.duration", deviceId), duration);
            return currentDeviceTrack;
        }

        public void Next(int deviceId)
        {
            if (!IsRegistered(deviceId)
                ||
                !Store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            QueueVote(deviceId);
            
            if (Store.ListLength(String.Format("musichub.device.{0}.queue", deviceId)) == 0)
            {
                return;
            }

            string currentDeviceTrack = UpdateState(deviceId);

            SendClientState(deviceId);

            if (currentDeviceTrack != null)
            {
                string deviceClientId = Store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
                Clients.Client(deviceClientId).SetTrackId(currentDeviceTrack);

                Play(deviceId);
            }
        }

        private void SendVoteOptions(int deviceId)
        {
            List<VoteOption> votes = new List<VoteOption>();

            IEnumerable<RedisKey> keys = redis.GetServer(redis.GetEndPoints()[0]).Keys(0, String.Format("musichub.device.{0}.vote.*", deviceId));
            foreach (RedisKey key in keys)
            {
                votes.Add(new VoteOption()
                {
                    TrackId = (int) Store.HashGet(key, "track"),
                    Votes = (int) Store.HashGet(key, "votes")
                });
            }
            Clients.Group(String.Format("device.{0}", deviceId)).SetVoteOptions(votes);
        }

        private void QueueVote(int deviceId)
        {
            IEnumerable<RedisKey> keys = redis.GetServer(redis.GetEndPoints()[0]).Keys(0, String.Format("musichub.device.{0}.vote.*", deviceId));
            int track = -1;
            int maxVotes = -1;

            foreach (RedisKey key in keys)
            {
                int votes = (int) Store.HashGet(key, "votes");
                if (votes > maxVotes)
                {
                    maxVotes = votes;
                    track = (int) Store.HashGet(key, "track");
                }
            }

            if (track != -1)
            {
                Store.ListRightPush(String.Format("musichub.device.{0}.queue", deviceId), track);
                string trackDeviceId = (from dt in this.dbContext.DeviceTracks
                                        where dt.DeviceId == deviceId && dt.TrackId == track
                                        select dt.DeviceTrackId).First();
                Store.ListRightPush(String.Format("musichub.device.{0}.queue.device", deviceId), trackDeviceId);
            }
            
            ResetVote(deviceId);
        }

        private void ResetVote(int deviceId)
        {
            IEnumerable<RedisKey> keys = redis.GetServer(redis.GetEndPoints()[0]).Keys(0, String.Format("musichub.device.{0}.vote.*", deviceId));
            foreach (RedisKey key in keys)
            {
                Store.KeyDelete(key);
            }
        }

        public void SubmitVoteOptions(int deviceId, int[] trackIds)
        {
            if (!IsRegistered(deviceId)
               ||
               !Store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            ResetVote(deviceId);

            foreach (int trackId in trackIds) 
            {
                Store.HashSet(String.Format("musichub.device.{0}.vote.{1}", deviceId, trackId), "track", trackId);
                Store.HashSet(String.Format("musichub.device.{0}.vote.{1}", deviceId, trackId), "votes", 0);
            }

            SendVoteOptions(deviceId);
        }

        public void VoteForTrack(int deviceId, int trackId)
        {
            if (!IsRegistered(deviceId)
               ||
               !Store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            Store.HashIncrement(String.Format("musichub.device.{0}.vote.{1}", deviceId, trackId), "votes");
            SendVoteOptions(deviceId);
        }
    }
}