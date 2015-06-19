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
    public class Track
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        
        [Required]
        [DataMember]
        [Index("IX_NameAndAlbum", 1)]
        [MaxLength(100)]
        public string Name { get; set; }

        [DataMember]
        public int Number { get; set; }
        [DataMember]
        public string MbId { get; set; }

        [Required]
        [DataMember]
        [Index("IX_NameAndAlbum", 2)]
        public int AlbumId { get; set; }
        public virtual Album Album { get; set; }

        public int? GenreId { get; set; }
        public virtual Genre Genre { get; set; }

        public virtual ICollection<DeviceTracks> DeviceTracks { get; set; }
    }
}
