using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace MusicPickerService.Models
{
    public class DeviceTracks
    {
        [Key, Column(Order = 0)]
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }

        [Key, Column(Order = 1)]
        public int TrackId { get; set; }
        public virtual Track Track { get; set; }

        public string DeviceTrackId { get; set; }
    }
}