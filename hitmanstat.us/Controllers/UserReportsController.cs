using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hitmanstat.us.Data;
using hitmanstat.us.Models;

namespace hitmanstat.us.Controllers
{
    public class UserReportsController : Controller
    {
        private readonly DatabaseContext _db;

        public UserReportsController(DatabaseContext context)
        {
            _db = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/report")]
        public async Task<IActionResult> Report(string reference, string fingerprint)
        {
            if (reference == null || fingerprint == null)
            {
                return BadRequest();
            }

            IPAddress address = Request.HttpContext.Connection.RemoteIpAddress;

            var count = (from r in _db.UserReports
                         where (r.IPAddressBytes == address.GetAddressBytes() 
                         || r.Fingerprint == fingerprint)
                         && r.Date > DateTime.Now.AddSeconds(-60) // TODO: Replace with .AddHours(-1)
                         select r).Count();

            if (count > 0)
            {
                return NoContent(); // TODO: Replace with Ok()
            }

            string service;

            switch (reference)
            {
                case "h2pc":
                    service = "HITMAN 2 PC";
                    break;
                case "h2xb":
                    service = "HITMAN 2 XBOX ONE";
                    break;
                case "h2ps":
                    service = "HITMAN 2 PS4";
                    break;
                case "h1pc":
                    service = "HITMAN PC";
                    break;
                case "h1xb":
                    service = "HITMAN XBOX ONE";
                    break;
                case "h1ps":
                    service = "HITMAN PS4";
                    break;
                default:
                    return BadRequest();
            }

            try
            {
                _db.Add(new UserReport
                {
                    IPAddress = address,
                    Fingerprint = fingerprint,
                    Service = service
                });
                await _db.SaveChangesAsync();

                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
