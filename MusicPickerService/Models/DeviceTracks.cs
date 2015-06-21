using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicPickerService.Models
{
    public class DeviceTracks
    {
        [Key, Column(Order = 0)]
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }

        [Key, Column(Order = 1)]
        [Index]
        public int TrackId { get; set; }
        public virtual Track Track { get; set; }

        public string DeviceTrackId { get; set; }
        public int TrackDuration { get; set; }
    }
}