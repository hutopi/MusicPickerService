using System;
using System.Collections.Generic;
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

        private IDatabase store
        {
            get
            {
                return redis.GetDatabase();
            }
        }

        private ApplicationUserManager userManager
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

            store.StringSet(String.Format("musichub.devices.{0}", Context.ConnectionId), deviceId);
            store.StringSet(String.Format("musichub.device.{0}.connection", deviceId), Context.ConnectionId);
        }

        [Authorize]
        public void RegisterClient(int deviceId)
        {
            Device device = dbContext.Devices.Find(deviceId);
            if (device.OwnerId != Context.User.Identity.GetUserId())
            {
                return;
            }

            store.SetAdd(String.Format("musichub.client.{0}.devices", Context.ConnectionId), deviceId);
            Groups.Add(Context.ConnectionId, String.Format("device.{0}", deviceId));
            store.SetAdd(String.Format("musichub.device.{0}.clients", deviceId), Context.ConnectionId);
        }

        public bool DeviceConnected(int deviceId)
        {
            return store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId));
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string deviceId;
            deviceId = store.StringGet(String.Format("musichub.devices.{0}", Context.ConnectionId));

            if (deviceId != null)
            {
                store.KeyDelete(String.Format("musichub.device.{0}.current", deviceId));
                store.KeyDelete(String.Format("musichub.device.{0}.duration", deviceId));
                store.KeyDelete(String.Format("musichub.device.{0}.playing", deviceId));
                store.KeyDelete(String.Format("musichub.device.{0}.lastpause", deviceId));
                store.KeyDelete(String.Format("musichub.device.{0}.queue", deviceId));
                store.KeyDelete(String.Format("musichub.device.{0}.queue.device", deviceId));
                store.KeyDelete(String.Format("musichub.device.{0}.clients", deviceId));
                store.KeyDelete(String.Format("musichub.device.{0}.connection", deviceId));
            }
            else
            {
                RedisValue[] members = store.SetMembers(String.Format("musichub.client.{0}.devices", Context.ConnectionId));
                foreach (string member in members)
                {
                    store.SetRemove(String.Format("musichub.client.{0}.devices", Context.ConnectionId), member);
                    Groups.Remove(Context.ConnectionId, String.Format("device.{0}", member));
                    store.SetRemove(String.Format("musichub.device.{0}.clients", member), Context.ConnectionId);
                }
            }

            return base.OnDisconnected(stopCalled);
        }

        private DeviceState CreateDeviceState(int deviceId)
        {
            return new DeviceState()
            {
                Current = (int)store.StringGet(String.Format("musichub.device.{0}.current", deviceId)),
                Duration = (int)store.StringGet(String.Format("musichub.device.{0}.duration", deviceId)),
                Playing = (bool)store.StringGet(String.Format("musichub.device.{0}.playing", deviceId)),
                LastPause = DateTime.FromFileTimeUtc((long)store.StringGet(String.Format("musichub.device.{0}.lastpause", deviceId))),
                Queue = (int[])Array.ConvertAll(store.ListRange(String.Format("musichub.device.{0}.queue", deviceId)), item => (int)item)
            };
        }
        private void SendClientState(int deviceId)
        {
            Clients.Group(String.Format("device.{0}", deviceId)).SetState(CreateDeviceState(deviceId));
        }

        private bool IsRegistered(int deviceId)
        {
            if (store.StringGet(String.Format("musichub.device.{0}.connection", deviceId)) == Context.ConnectionId)
            {
                return true;
            }

            return store.SetContains(String.Format("musichub.device.{0}.clients", deviceId), Context.ConnectionId);
        }

        public void Queue(int deviceId, int[] trackIds)
        {
            if (!IsRegistered(deviceId) 
                || 
                !store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            string queue = String.Format("musichub.device.{0}.queue", deviceId);
            string deviceQueue = String.Format("musichub.device.{0}.queue.device", deviceId);
            store.KeyDelete(queue);
            store.KeyDelete(deviceQueue);

            foreach (int trackId in trackIds)
            {
                try
                {
                    string trackDeviceId = (from dt in this.dbContext.DeviceTracks
                        where dt.DeviceId == deviceId && dt.TrackId == trackId
                        select dt.DeviceTrackId).First();
                    store.ListRightPush(queue, trackId);
                    store.ListRightPush(deviceQueue, trackDeviceId);
                }
                catch
                {
                    continue;
                }
            }

            SendClientState(deviceId);

            int current = (int)store.ListLeftPop(String.Format("musichub.device.{0}.queue", deviceId), 0);
            string currentDeviceTrack = store.ListLeftPop(String.Format("musichub.device.{0}.queue.device", deviceId), 0);

            string deviceClientId = store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Stop();
            Clients.Client(deviceClientId).SetTrackId(currentDeviceTrack);
        }

        public void GetState(int deviceId)
        {
            if (!IsRegistered(deviceId)
                ||
                !store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            Clients.Caller.SetState(CreateDeviceState(deviceId));
        }

        public void Play(int deviceId)
        {
            if (!IsRegistered(deviceId)
                ||
                !store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            bool sendTrack = (int) store.ListLength(String.Format("musichub.device.{0}.queue", deviceId)) == 0;
            QueueVote(deviceId);

            store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), true);
            store.StringSet(String.Format("musichub.device.{0}.lastpause", deviceId), DateTime.Now.ToFileTimeUtc());

            string deviceClientId = store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));

            if (sendTrack && store.ListLength(String.Format("musichub.device.{0}.queue", deviceId)) > 0)
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
                !store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), false);
            store.StringSet(String.Format("musichub.device.{0}.lastpause", deviceId), DateTime.Now.ToFileTimeUtc());

            SendClientState(deviceId);

            string deviceClientId = store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Pause();
        }

        public void RequestNext(int deviceId)
        {
            if (!IsRegistered(deviceId)
                ||
                !store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), false);
            store.KeyDelete(String.Format("musichub.device.{0}.lastpause", deviceId));

            SendClientState(deviceId);

            string deviceClientId = store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Stop();
        }

        private string UpdateState(int deviceId)
        {
            int current = (int) store.ListLeftPop(String.Format("musichub.device.{0}.queue", deviceId), 0);
            string currentDeviceTrack = store.ListLeftPop(String.Format("musichub.device.{0}.queue.device", deviceId), 0);
            store.StringSet(String.Format("musichub.device.{0}.current", deviceId), current);
            
            int duration = (from dt in this.dbContext.DeviceTracks
                                    where dt.DeviceId == deviceId && dt.TrackId == current
                                    select dt.TrackDuration).First();
            store.StringSet(String.Format("musichub.device.{0}.duration", deviceId), duration);
            return currentDeviceTrack;
        }

        public void Next(int deviceId)
        {
            if (!IsRegistered(deviceId)
                ||
                !store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            QueueVote(deviceId);
            
            if (store.ListLength(String.Format("musichub.device.{0}.queue", deviceId)) == 0)
            {
                return;
            }

            string currentDeviceTrack = UpdateState(deviceId);

            SendClientState(deviceId);

            if (currentDeviceTrack != null)
            {
                string deviceClientId = store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
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
                    TrackId = (int) store.HashGet(key, "track"),
                    Votes = (int) store.HashGet(key, "votes")
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
                int votes = (int) store.HashGet(key, "votes");
                if (votes > maxVotes)
                {
                    maxVotes = votes;
                    track = (int) store.HashGet(key, "track");
                }
            }

            if (track != -1)
            {
                store.ListRightPush(String.Format("musichub.device.{0}.queue", deviceId), track);
                string trackDeviceId = (from dt in this.dbContext.DeviceTracks
                                        where dt.DeviceId == deviceId && dt.TrackId == track
                                        select dt.DeviceTrackId).First();
                store.ListRightPush(String.Format("musichub.device.{0}.queue.device", deviceId), trackDeviceId);
            }
            
            ResetVote(deviceId);
        }

        private void ResetVote(int deviceId)
        {
            IEnumerable<RedisKey> keys = redis.GetServer(redis.GetEndPoints()[0]).Keys(0, String.Format("musichub.device.{0}.vote.*", deviceId));
            foreach (RedisKey key in keys)
            {
                store.KeyDelete(key);
            }
        }

        public void SubmitVoteOptions(int deviceId, int[] trackIds)
        {
            if (!IsRegistered(deviceId)
               ||
               !store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            ResetVote(deviceId);

            foreach (int trackId in trackIds) 
            {
                store.HashSet(String.Format("musichub.device.{0}.vote.{1}", deviceId, trackId), "track", trackId);
                store.HashSet(String.Format("musichub.device.{0}.vote.{1}", deviceId, trackId), "votes", 0);
            }

            SendVoteOptions(deviceId);
        }

        public void VoteForTrack(int deviceId, int trackId)
        {
            if (!IsRegistered(deviceId)
               ||
               !store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            store.HashIncrement(String.Format("musichub.device.{0}.vote.{1}", deviceId, trackId), "votes");
            SendVoteOptions(deviceId);
        }
    }
}