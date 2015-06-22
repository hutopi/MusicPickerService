// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-15-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="MusicHub.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
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

/// <summary>
/// The Hubs namespace.
/// </summary>
namespace MusicPickerService.Hubs
{
    /// <summary>
    /// Class MusicHub.
    /// </summary>
    public class MusicHub : Hub
    {
        /// <summary>
        /// The redis connection
        /// </summary>
        private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        /// <summary>
        /// The database context
        /// </summary>
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        /// <summary>
        /// Gets the store (database).
        /// </summary>
        /// <value>The store.</value>
        private IDatabase store
        {
            get
            {
                return redis.GetDatabase();
            }
        }

        /// <summary>
        /// Gets the user manager.
        /// </summary>
        /// <value>The user manager.</value>
        private ApplicationUserManager userManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        /// <summary>
        /// Registers the device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
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

        /// <summary>
        /// Registers the client.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
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

        /// <summary>
        /// Return if the device is connected or not
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool DeviceConnected(int deviceId)
        {
            return store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId));
        }

        /// <summary>
        /// Called when a connection disconnects from this hub gracefully or due to a timeout.
        /// </summary>
        /// <param name="stopCalled">true, if stop was called on the client closing the connection gracefully;
        /// false, if the connection has been lost for longer than the
        /// <see cref="P:Microsoft.AspNet.SignalR.Configuration.IConfigurationManager.DisconnectTimeout" />.
        /// Timeouts can be caused by clients reconnecting to another SignalR server in scaleout.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            string deviceId;
            deviceId = store.StringGet(String.Format("musichub.devices.{0}", Context.ConnectionId));

            if (deviceId != null)
            {
                store.KeyDelete(String.Format("musichub.device.{0}.current", deviceId));
                store.KeyDelete(String.Format("musichub.device.{0}.duration", deviceId));
                store.KeyDelete(String.Format("musichub.device.{0}.playing", deviceId));
                store.KeyDelete(String.Format("musichub.device.{0}.paused", deviceId));
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

        /// <summary>
        /// Creates the state of the device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>DeviceState.</returns>
        private DeviceState CreateDeviceState(int deviceId)
        {
            return new DeviceState()
            {
                Current = (int)store.StringGet(String.Format("musichub.device.{0}.current", deviceId)),
                Duration = (int)store.StringGet(String.Format("musichub.device.{0}.duration", deviceId)),
                Playing = (bool)store.StringGet(String.Format("musichub.device.{0}.playing", deviceId)),
                Paused = (bool)store.StringGet(String.Format("musichub.device.{0}.paused", deviceId)),
                LastPause = DateTime.FromFileTimeUtc((long)store.StringGet(String.Format("musichub.device.{0}.lastpause", deviceId))),
                Queue = (int[])Array.ConvertAll(store.ListRange(String.Format("musichub.device.{0}.queue", deviceId)), item => (int)item)
            };
        }
        /// <summary>
        /// Sends the state of the client.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        private void SendClientState(int deviceId)
        {
            Clients.Group(String.Format("device.{0}", deviceId)).SetState(CreateDeviceState(deviceId));
        }

        /// <summary>
        /// Determines whether the specified device identifier is registered.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns><c>true</c> if the specified device identifier is registered; otherwise, <c>false</c>.</returns>
        private bool IsRegistered(int deviceId)
        {
            if (store.StringGet(String.Format("musichub.device.{0}.connection", deviceId)) == Context.ConnectionId)
            {
                return true;
            }

            return store.SetContains(String.Format("musichub.device.{0}.clients", deviceId), Context.ConnectionId);
        }

        /// <summary>
        /// Queues the specified tracks on the device which has deviceId as id.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="trackIds">The track ids.</param>
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
            if ((bool) store.StringGet(String.Format("musichub.device.{0}.playing", deviceId)))
            {
                RequestNext(deviceId);
            }
            else
            {
                store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), false);
                Play(deviceId);
            }
        }

        /// <summary>
        /// Gets the state of the device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
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

        /// <summary>
        /// Plays the tracks on the specified device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        public void Play(int deviceId)
        {
            if (!IsRegistered(deviceId)
                ||
                !store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            bool sendTrack = (int)store.ListLength(String.Format("musichub.device.{0}.queue", deviceId)) == 0;
            QueueVote(deviceId);

            string deviceClientId = store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));

            if (store.ListLength(String.Format("musichub.device.{0}.queue", deviceId)) > 0)
            {
                store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), true);
                store.StringSet(String.Format("musichub.device.{0}.lastpause", deviceId), DateTime.Now.ToFileTimeUtc());


                if (!(bool) store.StringGet(String.Format("musichub.device.{0}.paused", deviceId)))
                {
                    Clients.Client(deviceClientId).Stop();
                    string currentDeviceTrack = UpdateState(deviceId);
                    Clients.Client(deviceClientId).SetTrackId(currentDeviceTrack);
                }
                else
                {
                    store.StringSet(String.Format("musichub.device.{0}.paused", deviceId), false);
                }

                Clients.Client(deviceClientId).Play();    
                SendClientState(deviceId);
            }
        }

        /// <summary>
        /// Pauses the musics on the specified device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        public void Pause(int deviceId)
        {
            if (!IsRegistered(deviceId)
                ||
                !store.KeyExists(String.Format("musichub.device.{0}.connection", deviceId)))
            {
                return;
            }

            store.StringSet(String.Format("musichub.device.{0}.playing", deviceId), false);
            store.StringSet(String.Format("musichub.device.{0}.paused", deviceId), true);
            store.StringSet(String.Format("musichub.device.{0}.lastpause", deviceId), DateTime.Now.ToFileTimeUtc());

            SendClientState(deviceId);

            string deviceClientId = store.StringGet(String.Format("musichub.device.{0}.connection", deviceId));
            Clients.Client(deviceClientId).Pause();
        }

        /// <summary>
        /// Requests the next song.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
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

        /// <summary>
        /// Updates the state.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>System.String.</returns>
        private string UpdateState(int deviceId)
        {
            int current = (int)store.ListLeftPop(String.Format("musichub.device.{0}.queue", deviceId), 0);
            string currentDeviceTrack = store.ListLeftPop(String.Format("musichub.device.{0}.queue.device", deviceId), 0);
            store.StringSet(String.Format("musichub.device.{0}.current", deviceId), current);

            int duration = (from dt in this.dbContext.DeviceTracks
                            where dt.DeviceId == deviceId && dt.TrackId == current
                            select dt.TrackDuration).First();
            store.StringSet(String.Format("musichub.device.{0}.duration", deviceId), duration);
            return currentDeviceTrack;
        }

        /// <summary>
        /// Play the next song on the specified device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
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

            Play(deviceId);
        }

        /// <summary>
        /// Sends the vote options.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        private void SendVoteOptions(int deviceId)
        {
            List<VoteOption> votes = new List<VoteOption>();

            IEnumerable<RedisKey> keys = redis.GetServer(redis.GetEndPoints()[0]).Keys(0, String.Format("musichub.device.{0}.vote.*", deviceId));
            foreach (RedisKey key in keys)
            {
                votes.Add(new VoteOption()
                {
                    TrackId = (int)store.HashGet(key, "track"),
                    Votes = (int)store.HashGet(key, "votes")
                });
            }
            Clients.Group(String.Format("device.{0}", deviceId)).SetVoteOptions(votes);
        }

        /// <summary>
        /// Queues the vote.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        private void QueueVote(int deviceId)
        {
            IEnumerable<RedisKey> keys = redis.GetServer(redis.GetEndPoints()[0]).Keys(0, String.Format("musichub.device.{0}.vote.*", deviceId));
            int track = -1;
            int maxVotes = -1;

            foreach (RedisKey key in keys)
            {
                int votes = (int)store.HashGet(key, "votes");
                if (votes > maxVotes)
                {
                    maxVotes = votes;
                    track = (int)store.HashGet(key, "track");
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

        /// <summary>
        /// Resets the vote.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        private void ResetVote(int deviceId)
        {
            IEnumerable<RedisKey> keys = redis.GetServer(redis.GetEndPoints()[0]).Keys(0, String.Format("musichub.device.{0}.vote.*", deviceId));
            foreach (RedisKey key in keys)
            {
                store.KeyDelete(key);
            }
        }

        /// <summary>
        /// Submits the vote options.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="trackIds">The track ids.</param>
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

        /// <summary>
        /// Votes for track.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="trackId">The track identifier.</param>
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