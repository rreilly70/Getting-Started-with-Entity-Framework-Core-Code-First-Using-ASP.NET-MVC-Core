using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.DAL;
using ContosoUniversity.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : Controller
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<Student> _logger;

        public CoursesController(UnitOfWork unitOfWork, ILogger<Student> logger)
        {
            this.unitOfWork = unitOfWork;
            this._logger = logger;
        }

        // GET: Courses
        public async Task<IActionResult> Index(int? SelectedDepartment)
        {
            var departments = unitOfWork.DepartmentRepository.DbSet.OrderBy(q => q.Name).ToList();
            ViewBag.SelectedDepartment = new SelectList(departments, "DepartmentID", "Name", SelectedDepartment);
            int departmentID = SelectedDepartment.GetValueOrDefault();

            IIncludableQueryable<Course, object> query = unitOfWork.context.Courses.Include(c => c.Department);
            Expression<Func<Course, bool>> filter = (c => !SelectedDepartment.HasValue || c.DepartmentID == departmentID);
            return View(await unitOfWork.CourseRepository.Get(query, filter, null, null));
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            IIncludableQueryable<Course, object> query = unitOfWork.context.Courses.Include(c => c.Department);
            var course = await unitOfWork.CourseRepository.GetById(query,id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDepartmentsDropDownList();
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseID,Credits,Title,DepartmentID")] Course course)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await unitOfWork.CourseRepository.Insert(course);
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes.  Try again, and if the problem persists see your system administrator.");
            }
            await PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //IIncludableQueryable<Course, object> query = unitOfWork.context.Courses.Include(s => s.Enrollments).ThenInclude(e => e.Course);

            var course = await unitOfWork.CourseRepository.GetById(null, id);
            if (course == null)
            {
                return NotFound();
            }
            await PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseID,Credits,Title,DepartmentID")] Course course)
        {
            if (id != course.CourseID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await unitOfWork.CourseRepository.Update(course);
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
            await PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        public ActionResult UpdateCourseCredits()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateCourseCredits(int? multiplier)
        {
            if (multiplier != null)
            {
                ViewBag.RowsAffected =  await unitOfWork.CourseRepository.context.Database.ExecuteSqlCommandAsync("UPDATE Courses SET Credits = Credits * @p0",default(System.Threading.CancellationToken) ,multiplier);
                    
            }
            return View();
        }

        private async Task<bool> PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            var departmentsQuery = await unitOfWork.DepartmentRepository.Get(null, null, dep => dep.OrderBy(d=>d.Name));
            ViewBag.DepartmentID = new SelectList (departmentsQuery, "DepartmentID", "Name", selectedDepartment);
            return true;
        }

        // GET: Courses/Delete/5
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

            var course = await unitOfWork.CourseRepository.GetById(null, id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await unitOfWork.CourseRepository.Delete(id);
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
