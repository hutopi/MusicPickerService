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
    public class Artist
    {
        [Key]
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string MbId { get; set; }
      
        public virtual ICollection<Album> Albums { get; set; }
    }
}
