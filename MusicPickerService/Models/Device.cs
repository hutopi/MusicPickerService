using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MusicPickerService.Models
{
    public class Device
    {
        [Key]
        public int Id { get; set; }

        [IgnoreDataMember]
        public int OwnerID { get; set; }

        [IgnoreDataMember]
        public int Owner { get; set; } // owner @TODO

        public DateTime RegistrationDate { get; set; }
        public DateTime AccessDate { get; set; }
        public string Name { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<Track> Tracks { get; set; }
    }
}
