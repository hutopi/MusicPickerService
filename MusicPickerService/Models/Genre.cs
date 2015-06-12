using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MusicPickerService.Models
{
    public class Genre
    {
        [Key]
        public int GenreID { get; set; }
        public string Name { get; set; }
    }
}
