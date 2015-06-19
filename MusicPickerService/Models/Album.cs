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
    public class Album
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        [Index("IX_NameAndArtist", 1)]
        [MaxLength(100)]
        public string Name { get; set; }

        [DataMember]
        public int Year { get; set; }
        [DataMember]
        public string MbId { get; set; }

        [Required]
        [DataMember]
        [Index("IX_NameAndArtist", 2)]
        public int ArtistId { get; set; }
        public virtual Artist Artist { get; set; }

        public virtual ICollection<Track> Tracks { get; set; }
    }
}
