using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MusicPickerService.Models
{
    public class Track
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        public int Number { get; set; }
        public string MbId { get; set; }

        [Required]
        public int AlbumId { get; set; }
        public virtual Album Album { get; set; }

        public int GenreId { get; set; }
        public virtual Genre Genre { get; set; }

        public virtual ICollection<DeviceTracks> DeviceTracks { get; set; }
    }
}
