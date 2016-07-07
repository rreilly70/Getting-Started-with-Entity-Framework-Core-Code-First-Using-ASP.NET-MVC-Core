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
using ContosoUniversity.ViewModels;

namespace ContosoUniversity.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<Instructor> _logger;

        public InstructorsController(UnitOfWork unitOfWork, ILogger<Instructor> logger)
        {
            this.unitOfWork = unitOfWork;
            this._logger = logger;
        }

        // GET: Instructors
        public async Task<IActionResult> Index(int? id, int? courseID)
        {
            var viewModel = new InstructorIndexData();
            IIncludableQueryable<Instructor, object> query = unitOfWork.context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseInstructors)
                    .ThenInclude(ci => ci.Course)
                    .ThenInclude(c => c.Department)
                .Include(i => i.CourseInstructors)
                    .ThenInclude(ci => ci.Course)
                    .ThenInclude(c => c.Enrollments)
                    .ThenInclude(e => e.Student);

            viewModel.Instructors = await query.ToListAsync();

            if (id != null)
            {
                ViewBag.InstructorID = id.Value;
                List<Course> courseList = new List<Course>();
                foreach (var ci in viewModel.Instructors.SingleOrDefault(i => i.ID == id.Value).CourseInstructors)
                {
                    courseList.Add(ci.Course);
                }
                viewModel.Courses = courseList;
            }
            if (courseID != null)
            {
                ViewBag.CourseID = courseID.Value;
                viewModel.Enrollments = viewModel.Courses.Where(
                x => x.CourseID == courseID).Single().Enrollments;
            }
            return View(viewModel);
        }

        // GET: Instructors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await unitOfWork.InstructorRepository.GetById(null, id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // GET: Instructors/Create
        public async Task<IActionResult> Create()
        {
            var instructor = new Instructor();
            instructor.CourseInstructors = new List<CourseInstructor>();
            instructor.CourseInstructors.Add(new CourseInstructor());
            await PopulateAssignedCourseData(instructor);
            return View();
        }

        // POST: Instructors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstMidName,HireDate,LastName,OfficeAssignment")] Instructor instructor, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.CourseInstructors = new List<CourseInstructor>();
                foreach(var course in selectedCourses)
                {
                    var courseToAdd = await unitOfWork.CourseRepository.GetById(null, int.Parse(course));
                    instructor.CourseInstructors.Add(new CourseInstructor
                    {
                        InstructorId = instructor.ID,
                        Instructor=instructor,
                        CourseId=courseToAdd.CourseID,
                        Course=courseToAdd
                    });
                }
            }
            try
            {
                if (ModelState.IsValid)
                {
                    await unitOfWork.InstructorRepository.Insert(instructor);
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes.  Try again, and if the problem persists see your system administrator.");
            }
            await PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: Instructors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            IIncludableQueryable<Instructor, object> query = unitOfWork.context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseInstructors)
                    .ThenInclude(Ci => Ci.Course);


            var instructor = await unitOfWork.InstructorRepository.GetById(query, id);

            if (instructor == null)
            {
                return NotFound();
            }
            await PopulateAssignedCourseData(instructor);
            return View(instructor);
        }



        // POST: Instructors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return BadRequest();
            }

            IIncludableQueryable<Instructor, object> query = unitOfWork.context.Instructors.Include(i => i.OfficeAssignment).Include(i => i.CourseInstructors).ThenInclude(ci => ci.Course);
            var instructorToUpdate = await unitOfWork.InstructorRepository.GetById(query, id);

            if (await TryUpdateModelAsync(instructorToUpdate))
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment.Location))
                    {
                        instructorToUpdate.OfficeAssignment = null;
                    }

                    await UpdateInstructorCourses(selectedCourses, instructorToUpdate);
                    await unitOfWork.InstructorRepository.Update(instructorToUpdate);

                    return RedirectToAction("Index");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes.  Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(instructorToUpdate);
        }

        // GET: Instructors/Delete/5
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

            var instructor = await unitOfWork.InstructorRepository.GetById(null, id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // POST: Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            IIncludableQueryable<Instructor, object> query = unitOfWork.context.Instructors.Include(i => i.OfficeAssignment);
            var instructorToDelete = await unitOfWork.InstructorRepository.GetById(query, id);
           
            var department = await unitOfWork.DepartmentRepository.GetById(null,id);
            if (department != null)
            {
                department.InstructorID = null;
            }
            instructorToDelete.OfficeAssignment = null;

            try
            {
                await unitOfWork.InstructorRepository.Delete(instructorToDelete);
            }
            catch (DbUpdateException)
            {
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }

        private async Task<bool> PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = await unitOfWork.CourseRepository.Get(null, null, null);
            var instructorCourses = new HashSet<int>(instructor.CourseInstructors.Select(c => c.CourseId));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewBag.Courses = viewModel; ;
            return true;
        }

        private async Task<bool> UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.CourseInstructors = new List<CourseInstructor>();
                return true;
            }
            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>(instructorToUpdate.CourseInstructors.Select(ci => ci.CourseId));

            var courselist = await unitOfWork.CourseRepository.Get(null, null, null);

            foreach (var course in courselist)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.CourseInstructors.Add(new CourseInstructor { CourseId = course.CourseID, Course = course, InstructorId = instructorToUpdate.ID, Instructor = instructorToUpdate });
                    }
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.CourseInstructors.RemoveAll(ci => ci.CourseId == course.CourseID);

                    }
                }
            }
            return true;
        }


        protected override void Dispose(bool disposing)
        {
            unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
}
