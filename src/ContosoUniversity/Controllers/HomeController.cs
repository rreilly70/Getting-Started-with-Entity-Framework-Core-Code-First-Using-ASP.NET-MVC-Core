using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContosoUniversity.DAL;
using ContosoUniversity.ViewModels;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Models;

namespace ContosoUniversity.Controllers
{
    public class HomeController : Controller
    {
        private SchoolContext db;
        private UnitOfWork unitOfWork;

        public HomeController(SchoolContext context, UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            db = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> About()
        {
            // Commenting out LINQ to show how to do the same thing in SQL.
            var students = await unitOfWork.StudentRepository.Get(null, null, null);
            var data = students.GroupBy(s => s.EnrollmentDate)
                .Select(e => new EnrollmentDateGroup()
                {
                    EnrollmentDate = e.Key,
                    StudentCount = e.Count()
                });

            // SQL version of the above LINQ code.
            // EF Core 1.0 does not curently support enable returning ad-hoc types from raw SQL queries.

            return View(data.ToList());

        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
}
