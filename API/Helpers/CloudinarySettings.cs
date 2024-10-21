using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class CloudinarySettings
    {
        public required string Cloudname { get; set; }
        public required string ApiKey { get; set; }
        public required string ApiSecret { get; set; }
    }
}