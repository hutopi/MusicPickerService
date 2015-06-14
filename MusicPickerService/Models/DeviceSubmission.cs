using System;

namespace MusicPickerService.Models
{
    public class DeviceSubmission
    {
        public string Id { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public uint Year { get; set; }
        public uint Number { get; set; }
        public uint Count { get; set; }
    }
}