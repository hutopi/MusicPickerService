using System;

namespace MusicPickerService.Models
{
    public class DeviceState
    {
        public int Id { get; set; }
        public bool Playing { get; set; }
        public bool Paused { get; set; }
        public DateTime LastPause { get; set; }
        public int[] Queue { get; set; }
        public String[] DeviceQueue { get; set; }
    }
}