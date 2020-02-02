using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HitmanAPI.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [Route("status")]
        public JsonResult Get()
        {
            byte[] debug = Properties.Resources.debug;
            var status = ReadResourceFile(debug);

            var json = JObject.Parse(status);
            json["timestamp"] = DateTime.UtcNow;

            return new JsonResult(json);
        }

        private string ReadResourceFile(byte[] resource)
        {
            using Stream stream = new MemoryStream(resource);
            using StreamReader reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
