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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace ContosoUniversity.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<Instructor> _logger;

        public DepartmentsController(UnitOfWork unitOfWork, ILogger<Instructor> logger)
        {
            this.unitOfWork = unitOfWork;
            this._logger = logger;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {

            IIncludableQueryable<Department, object> query = unitOfWork.context.Departments.Include(d => d.Administrator);
            var departments = await unitOfWork.DepartmentRepository.Get(query, null, null);
            return View(departments);
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Commenting out original code to show how to use a raw SQL query.
            //var department = await unitOfWork.DepartmentRepository.GetById(null, id);

            // Create and execute raw SQL query
            string query = "SELECT * FROM Departments WHERE DepartmentID = @p0";
            Department department = await unitOfWork.DepartmentRepository.DbSet.FromSql(query, id).SingleOrDefaultAsync();

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        public async Task<IActionResult> Create()
        {
            ViewData["InstructorID"] = new SelectList(await unitOfWork.InstructorRepository.Get(null,null,null), "ID", "FirstMidName");
            return View();
        }

        // POST: Departments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentID,Budget,InstructorID,Name,StartDate")] Department department)
        {
            if (ModelState.IsValid)
            {
                await unitOfWork.DepartmentRepository.Insert(department);
                return RedirectToAction("Index");
            }
            ViewData["InstructorID"] = new SelectList(await unitOfWork.InstructorRepository.Get(null, null, null), "ID", "FirstMidName", department.InstructorID);
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await unitOfWork.DepartmentRepository.GetById(null,id);
            if (department == null)
            {
                return NotFound();
            }
            ViewData["InstructorID"] = new SelectList(await unitOfWork.InstructorRepository.Get(null, null, null), "ID", "FullName", department.InstructorID);
            return View(department);
        }

        // POST: Departments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentID,Budget,InstructorID,Name,RowVersion,StartDate")] Department department)
        {
            if (ModelState.IsValid)
            {
                await ValidateOneAdministratorAssignmentPerInstructor(department);
            }
            if (id != department.DepartmentID)
            {
                return NotFound();
            }
            try
            {
                if (ModelState.IsValid)
                {

                    await unitOfWork.DepartmentRepository.Update(department);
                    return RedirectToAction("Index");
                }
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var clientValues = (Department)entry.Entity;
                var databaseEntityCount = unitOfWork.context.Departments.AsNoTracking().Count(d => d.DepartmentID == clientValues.DepartmentID);
                if (databaseEntityCount == 0)
                {
                    ModelState.AddModelError(string.Empty,"Unable to save changes. The department was deleted by another user.");
                }
                else
                {
                    var databaseValues = unitOfWork.context.Departments.AsNoTracking().Single(d => d.DepartmentID == clientValues.DepartmentID);
                    if (databaseValues.Name != clientValues.Name)
                        ModelState.AddModelError("Name", "Current value: "
                        + databaseValues.Name);
                    if (databaseValues.Budget != clientValues.Budget)
                        ModelState.AddModelError("Budget", "Current value: "
                        + String.Format("{0:c}", databaseValues.Budget));
                    if (databaseValues.StartDate != clientValues.StartDate)
                        ModelState.AddModelError("StartDate", "Current value: "
                        + String.Format("{0:d}", databaseValues.StartDate));
                    if (databaseValues.InstructorID != clientValues.InstructorID)
                        ModelState.AddModelError("InstructorID", "Current value: "
                        + (await unitOfWork.InstructorRepository.GetById(null,databaseValues.InstructorID)).FullName);
                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                    + "was modified by another user after you got the original value. The "
                    + "edit operation was canceled and the current values in the database "
                    + "have been displayed. If you still want to edit this record, click "
                    + "the Save button again. Otherwise click the Back to List hyperlink.");
                    department.RowVersion = databaseValues.RowVersion;
                    ModelState.Remove("RowVersion");
                }
                
            }
            
            ViewData["InstructorID"] = new SelectList(await unitOfWork.InstructorRepository.Get(null, null, null), "ID", "FullName", department.InstructorID);
            
            return   View(department);
            
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id,bool? concurrencyError)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var department = await unitOfWork.DepartmentRepository.GetById(null, id);
            if (department == null)
            {
                return NotFound();
            }
            if (concurrencyError.GetValueOrDefault())
            {
                if (department == null)
                {
                    ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                    + "was deleted by another user after you got the original values. "
                    + "Click the Back to List hyperlink.";
                }
                else
                {
                    ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                    + "was modified by another user after you got the original values. "
                    + "The delete operation was canceled and the current values in the "
                    + "database have been displayed. If you still want to delete this "
                    + "record, click the Delete button again. Otherwise "
                    + "click the Back to List hyperlink.";
                }
            }
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Department department)
        {
            try
            {
                await unitOfWork.DepartmentRepository.Delete(department);
                return RedirectToAction("Index");
            }
            catch(DbUpdateConcurrencyException)
            {
                return RedirectToAction("Delete", new { concurrencyError = true });
            }
            catch(DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                return View(department);
            }
        }
        private async Task<bool> ValidateOneAdministratorAssignmentPerInstructor(Department department)
        {
            if (department.InstructorID != null)
            {
                Department duplicateDepartment = await unitOfWork.context.Departments.Include(d => d.Administrator).Where(d=>d.InstructorID==department.InstructorID).AsNoTracking().FirstOrDefaultAsync();

                if (duplicateDepartment != null && duplicateDepartment.DepartmentID != department.DepartmentID)
                {
                    string errorMessage = String.Format(
                    "Instructor {0} {1} is already administrator of the {2} department.",
                    duplicateDepartment.Administrator.FirstMidName,
                    duplicateDepartment.Administrator.LastName,
                    duplicateDepartment.Name);
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
                return true;
            }
            return false;
        }
        protected override void Dispose(bool disposing)
        {
            unitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
}
