using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MusicPickerService.Models
{
    [DataContract]
    public class Album
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public uint Year { get; set; }
        [DataMember]
        public string MbId { get; set; }

        [Required]
        [DataMember]
        public int ArtistId { get; set; }
        public virtual Artist Artist { get; set; }

        public virtual ICollection<Track> Tracks { get; set; }
    }
}
