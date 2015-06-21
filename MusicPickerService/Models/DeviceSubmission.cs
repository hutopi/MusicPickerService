namespace MusicPickerService.Models
{
    public class DeviceSubmission
    {
        public string Id { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public int Number { get; set; }
        public uint Count { get; set; }
        public int Duration { get; set; }
    }
}