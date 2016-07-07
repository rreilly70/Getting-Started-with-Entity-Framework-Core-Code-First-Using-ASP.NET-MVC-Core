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

namespace ContosoUniversity.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<Student> _logger;

        public EnrollmentsController(UnitOfWork unitOfWork, ILogger<Student> logger)
        {
            this.unitOfWork = unitOfWork;
            this._logger = logger;
        }

        // GET: enrollments
        public async Task<IActionResult> Index()
        {
            return View(await unitOfWork.EnrollmentRepository.Get(null, null, null, null));
        }

        // GET: enrollments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await unitOfWork.EnrollmentRepository.GetById(null, id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: enrollments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: enrollments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnrollmentID, CourseID, StudentID")] Enrollment enrollment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await unitOfWork.EnrollmentRepository.Insert(enrollment);
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes.  Try again, and if the problem persists see your system administrator.");
            }
            return View(enrollment);
        }

        // GET: enrollments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //IIncludableQueryable<enrollment, object> query = unitOfWork.context.enrollments.Include(s => s.Enrollments).ThenInclude(e => e.enrollment);

            var enrollment = await unitOfWork.EnrollmentRepository.GetById(null, id);
            if (enrollment == null)
            {
                return NotFound();
            }
            return View(enrollment);
        }

        // POST: enrollments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EnrollmentID, CourseID, StudentID")] Enrollment enrollment)
        {
            if (id != enrollment.EnrollmentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await unitOfWork.EnrollmentRepository.Update(enrollment);
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
            return View(enrollment);
        }

        // GET: enrollments/Delete/5
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

            //IIncludableQueryable<Student, object> query = unitOfWork.context.Students.Include(s => s.Enrollments).ThenInclude(e => e.enrollment);

            var enrollment = await unitOfWork.EnrollmentRepository.GetById(null, id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await unitOfWork.EnrollmentRepository.Delete(id);
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
