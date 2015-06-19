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
    public class Artist
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        [Index]
        [MaxLength(100)]
        public string Name { get; set; }

        [DataMember]
        public string MbId { get; set; }
      
        public virtual ICollection<Album> Albums { get; set; }
    }
}
