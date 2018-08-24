using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FHCore.MVC.Models;

namespace FHCore.MVC.Controllers
{
    public class HomeController : Controller
    {
        public ILog _log{get;set;}

        public IActionResult Index()
        {
            _log.Write("进入Index页面");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public interface ILog
    {
        void Write(string log);
    }
    public class MyLog : ILog
    {
        public void Write(string log)
        {
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
            Console.Write(log);
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
        }
    }
}
