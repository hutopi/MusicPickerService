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

        public int GenreID { get; set; }
        public Genre Genre { get; set; }

        public int AlbumID { get; set; }
        public Album Album { get; set; }

        public int Number { get; set; }

        public virtual ICollection<Device> Devices { get; set; }
    }
}
