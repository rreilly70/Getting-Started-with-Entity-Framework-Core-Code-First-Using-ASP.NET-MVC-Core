using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.DAL;
using ContosoUniversity.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using Sakura.AspNetCore;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ContosoUniversity.Controllers
{
    public class StudentsController : Controller
    {
        //private IStudentRepository studentRepository;
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<Student> _logger;
        
        public StudentsController(UnitOfWork unitOfWork, ILogger<Student> logger)
        {
            this.unitOfWork = unitOfWork;
            this._logger = logger;
        }

        // GET: Students
        public async Task<IActionResult> Index(string sortOrder,string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            IIncludableQueryable<Student, object> query = unitOfWork.context.Students.Include(s => s.Enrollments).ThenInclude(e => e.Course);
            List<Student> students;
            Expression<Func<Student, bool>> filter = null;
            
            if (!String.IsNullOrEmpty(searchString))
            {
                filter = (s) => s.LastName.ToUpper().Contains(searchString.ToUpper())|| s.FirstMidName.ToUpper().Contains(searchString.ToUpper());
            }        

            switch (sortOrder)
            {
                case "name_desc":
                    students = await unitOfWork.StudentRepository.Get(null,filter,q => q.OrderByDescending(s => s.LastName),null,null);
                    break;
                case "Date":
                    students = await unitOfWork.StudentRepository.Get(null,filter, q => q.OrderBy(s => s.EnrollmentDate),null, null);
                    break;
                case "date_desc":
                    students = await unitOfWork.StudentRepository.Get(null, filter, q => q.OrderByDescending(s => s.EnrollmentDate), null, null);
                    break;
                default:
                    students = await unitOfWork.StudentRepository.Get(null, filter, q => q.OrderBy(s => s.LastName), null, null);
                    break;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            _logger.LogInformation("Students {@students}",students);
            return View(students.ToPagedList(pageSize, pageNumber));

        }
        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            IIncludableQueryable<Student, object> query = unitOfWork.context.Students.Include(s => s.Enrollments).ThenInclude(e => e.Course);

            var student = await unitOfWork.StudentRepository.GetById(query, id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnrollmentDate,FirstMidName,LastName")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await unitOfWork.StudentRepository.Insert(student);
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes.  Try again, and if the problem persists see your system administrator.");

            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //IIncludableQueryable<Student, object> query = unitOfWork.context.Students.Include(s => s.Enrollments).ThenInclude(e => e.Course);

            var student = await unitOfWork.StudentRepository.GetById(null, id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,EnrollmentDate,FirstMidName,LastName")] Student student)
        {
            if (id != student.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await unitOfWork.StudentRepository.Update(student);
                }
                catch (NotFoundException)
                {
                    return NotFound();
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes.  Try again, and if the problem persists see your system administrator.");
                }
                return RedirectToAction("Index");
            }
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }

            //IIncludableQueryable<Student, object> query = unitOfWork.context.Students.Include(s => s.Enrollments).ThenInclude(e => e.Course);

            var student = await unitOfWork.StudentRepository.GetById(null, id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await unitOfWork.StudentRepository.Delete(id);
            }
            catch (DbUpdateException)
            {
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            unitOfWork.Dispose();
            base.Dispose(disposing);
        }

    }
}
