using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MusicPickerService.Models
{
    public class Device
    {
        [Key]
        public int DeviceID { get; set; }

        public int OwnerID { get; set; }
        public int Owner { get; set; } // owner @TODO

        public DateTime RegistrationDate { get; set; }
        public DateTime AccessDate { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Track> Tracks { get; set; }

    }
}
