using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MusicPickerService.Models
{
    public class Artist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string MbId { get; set; }
      
        public virtual ICollection<Album> Albums { get; set; }
    }
}
