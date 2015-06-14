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
    public class Track
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        
        [Required]
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Number { get; set; }
        [DataMember]
        public string MbId { get; set; }

        [Required]
        [DataMember]
        public int AlbumId { get; set; }
        public virtual Album Album { get; set; }

        public int? GenreId { get; set; }
        public virtual Genre Genre { get; set; }

        public virtual ICollection<DeviceTracks> DeviceTracks { get; set; }
    }
}
