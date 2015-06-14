using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MusicPickerService.Models
{
    public class Album
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public uint Year { get; set; }
        public string MbId { get; set; }

        [Required]
        public int ArtistId { get; set; }
        public virtual Artist Artist { get; set; }

        public virtual ICollection<Track> Tracks { get; set; }
    }
}
