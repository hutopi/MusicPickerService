using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace MusicPickerService.Models
{
    [DataContract]
    public class Device
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        public string OwnerId { get; set; }
        public virtual ApplicationUser Owner { get; set; }

        [DataMember]
        public DateTime RegistrationDate { get; set; }

        [DataMember]
        public DateTime AccessDate { get; set; }

        [DataMember]
        public string Name { get; set; }

        public virtual ICollection<Track> Tracks { get; set; }
    }
}
