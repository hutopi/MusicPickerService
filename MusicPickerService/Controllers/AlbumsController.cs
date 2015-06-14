using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MusicPickerService.Controllers
{
    public class AlbumsController : ApiController
    {
        [HttpGet]
        public IEnumerable<string> GetByDevice(int device)
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        public IEnumerable<string> GetByDeviceAndArtist(int device, int artist)
        {
            return new string[] { "value3", "value4" };
        }

        [HttpGet]
        public string Get(int id)
        {
            return "value";
        }
    }
}
