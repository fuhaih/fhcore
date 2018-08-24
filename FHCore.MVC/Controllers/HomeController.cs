﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FHCore.MVC.Models;
using Autofac;
namespace FHCore.MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            using(var scope=Startup.ApplicationContainer.BeginLifetimeScope())
            {
                for(int i=0;i<10;i++)
                {
                    ITest test=scope.Resolve<ITest>();
                    test.Write();
                }
            }
            using(var scope2=Startup.ApplicationContainer.BeginLifetimeScope())
            {
                for(int i=0;i<10;i++)
                {
                    ITest test=scope2.Resolve<ITest>();
                    test.Write();
                }
            }
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

    public interface ITest{
        void Write();
    }

    public class MyTest:ITest
    {
        public string GUID;
        public MyTest()
        {
            GUID= Guid.NewGuid().ToString();

        }
        public void Write()
        {
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine(GUID);
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------");
        }
    }

}
