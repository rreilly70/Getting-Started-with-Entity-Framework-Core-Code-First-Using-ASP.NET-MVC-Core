using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.DAL
{
    public class UnitOfWork : ActionFilterAttribute, IDisposable
    {
        public SchoolContext context;
        private GenericRepository<Department> departmentRepository;
        private GenericRepository<Student> studentRepository;
        private GenericRepository<Course> courseRepository;
        private GenericRepository<Enrollment> enrollmentRepository;
        private GenericRepository<Instructor> instructorRepository;
        private GenericRepository<OfficeAssignment> officeAssignmentsRepository;

        public UnitOfWork(SchoolContext context)
        {
            this.context = context;
            
        }
        public UnitOfWork() : base() { }
        public override async Task OnActionExecutionAsync(ActionExecutingContext executingContext, ActionExecutionDelegate next)
        {
            var executedContext = await next.Invoke(); //to wait until the controller's action finalizes in case there was an error
            if (executedContext.Exception == null)
            {

                await context.SaveChangesAsync();
            }

        }
        public GenericRepository<Department> DepartmentRepository
        {
            get
            {

                if (this.departmentRepository == null)
                {
                    this.departmentRepository = new GenericRepository<Department>(context);
                }
                return departmentRepository;
            }
        }
        public GenericRepository<Student> StudentRepository
        {
            get
            {

                if (this.studentRepository == null)
                {
                    this.studentRepository = new GenericRepository<Student>(context);
                }
                return studentRepository;
            }
        }
        public GenericRepository<Course> CourseRepository
        {
            get
            {

                if (this.courseRepository == null)
                {
                    this.courseRepository = new GenericRepository<Course>(context);
                }
                return courseRepository;
            }
        }

        public GenericRepository<Enrollment> EnrollmentRepository
        {
            get
            {

                if (this.enrollmentRepository == null)
                {
                    this.enrollmentRepository = new GenericRepository<Enrollment>(context);
                }
                return enrollmentRepository;
            }
        }
        public GenericRepository<Instructor> InstructorRepository
        {
            get
            {

                if (this.instructorRepository == null)
                {
                    this.instructorRepository = new GenericRepository<Instructor>(context);
                }
                return instructorRepository;
            }
        }

        public GenericRepository<OfficeAssignment> OfficeAssignmentsRepository
        {
            get
            {

                if (this.officeAssignmentsRepository == null)
                {
                    this.officeAssignmentsRepository = new GenericRepository<OfficeAssignment>(context);
                }
                return officeAssignmentsRepository;
            }
        }


        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
