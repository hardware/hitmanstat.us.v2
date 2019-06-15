﻿using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hitmanstat.us.Models;
using hitmanstat.us.Data;

namespace hitmanstat.us.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseContext _db;

        public HomeController(DatabaseContext context) 
            => _db = context;

        public IActionResult Index()
        {
            return View();
        }

        [Route("/events")]
        [Route("/events/{days:int:range(1,100)}")]
        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "days" })]
        public async Task<IActionResult> Events(int days = 7)
        {
            ViewBag.days = days;

            var events = (from e in _db.Events where e.Date > DateTime.Now.AddDays(-days) select e)
                            .OrderByDescending(e => e.ID)
                            .Take(300)
                            .ToListAsync();

            return View(await events);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [Route("error/{code:int}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int code)
        {
            return View(new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorCode = code
            });
        }
    }
}
