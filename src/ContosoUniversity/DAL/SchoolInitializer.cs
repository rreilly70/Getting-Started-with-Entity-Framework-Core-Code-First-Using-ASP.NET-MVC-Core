using ContosoUniversity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContosoUniversity.DAL
{
    public class SchoolInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new SchoolContext(
                    serviceProvider.GetRequiredService<DbContextOptions<SchoolContext>>()))
            {
                context.Database.Migrate();

                if (context.Students.Any() || context.Enrollments.Any() || context.Courses.Any() || context.Departments.Any() || context.Instructors.Any() || context.OfficeAssignments.Any())
                {
                    return;
                }
                context.Students.AddRange(
                    new Student { FirstMidName = "Carson", LastName = "Alexander", EnrollmentDate = DateTime.Parse("2005-09-01") },
                    new Student { FirstMidName = "Meredith", LastName = "Alonso", EnrollmentDate = DateTime.Parse("2002-09-01") },
                    new Student { FirstMidName = "Arturo", LastName = "Anand", EnrollmentDate = DateTime.Parse("2003-09-01") },
                    new Student { FirstMidName = "Gytis", LastName = "Barzdukas", EnrollmentDate = DateTime.Parse("2002-09-01") },
                    new Student { FirstMidName = "Yan", LastName = "Li", EnrollmentDate = DateTime.Parse("2002-09-01") },
                    new Student { FirstMidName = "Peggy", LastName = "Justice", EnrollmentDate = DateTime.Parse("2001-09-01") },
                    new Student { FirstMidName = "Laura", LastName = "Norman", EnrollmentDate = DateTime.Parse("2003-09-01") },
                    new Student { FirstMidName = "Nino", LastName = "Olivetto", EnrollmentDate = DateTime.Parse("2005-09-01") }
                    );

                var instructors = new List<Instructor>() {
                    new Instructor{FirstMidName = "Kim", LastName = "Abercrombie", HireDate = DateTime.Parse("1995-03-11"),},
                    new Instructor{FirstMidName = "Fadi", LastName = "Fakhouri", HireDate = DateTime.Parse("2002-07-06")},
                    new Instructor{ FirstMidName = "Roger", LastName = "Harui", HireDate = DateTime.Parse("1998-07-01")},
                    new Instructor{ FirstMidName = "Candace", LastName = "Kapoor", HireDate = DateTime.Parse("2001-01-15")},
                    new Instructor{ FirstMidName = "Roger", LastName = "Zheng", HireDate = DateTime.Parse("2004-02-12")}
                    };
                context.Instructors.AddRange(instructors);

                var departments = new List<Department>() {
                    new Department{ Name = "English", Budget = 350000, StartDate = DateTime.Parse("2007-09-01"), InstructorID = instructors.Single(i => i.LastName == "Abercrombie").ID},
                    new Department{ Name = "Mathematics", Budget = 100000, StartDate = DateTime.Parse("2007-09-01"), InstructorID = instructors.Single(i => i.LastName == "Fakhouri").ID},
                    new Department{ Name = "Engineering", Budget = 350000, StartDate = DateTime.Parse("2007-09-01"), InstructorID = instructors.Single(i => i.LastName == "Harui").ID},
                    new Department{ Name = "Economics", Budget = 100000, StartDate = DateTime.Parse("2007-09-01"), InstructorID = instructors.Single(i => i.LastName == "Kapoor").ID}
                    };
                context.Departments.AddRange(departments);

                context.Courses.AddRange(
                    new Course { CourseID = 1050, Title = "Chemistry", Credits = 3, DepartmentID = departments.Single(s => s.Name == "Engineering").DepartmentID, CourseInstructors = new List<CourseInstructor>() },
                    new Course { CourseID = 4022, Title = "Microeconomics", Credits = 3, DepartmentID = departments.Single(s => s.Name == "Economics").DepartmentID, CourseInstructors = new List<CourseInstructor>() },
                    new Course { CourseID = 4041, Title = "Macroeconomics", Credits = 3, DepartmentID = departments.Single(s => s.Name == "Economics").DepartmentID, CourseInstructors = new List<CourseInstructor>() },
                    new Course { CourseID = 1045, Title = "Calculus", Credits = 4, DepartmentID = departments.Single(s => s.Name == "Mathematics").DepartmentID, CourseInstructors = new List<CourseInstructor>() },
                    new Course { CourseID = 3141, Title = "Trigonometry", Credits = 4, DepartmentID = departments.Single(s => s.Name == "Mathematics").DepartmentID, CourseInstructors = new List<CourseInstructor>() },
                    new Course { CourseID = 2021, Title = "Composition", Credits = 3, DepartmentID = departments.Single(s => s.Name == "English").DepartmentID, CourseInstructors = new List<CourseInstructor>() },
                    new Course { CourseID = 2042, Title = "Literature", Credits = 4, DepartmentID = departments.Single(s => s.Name == "English").DepartmentID, CourseInstructors = new List<CourseInstructor>() }
                    );

                var officeAssignments = new List<OfficeAssignment>
                {
                    new OfficeAssignment { InstructorID = instructors.Single( i => i.LastName == "Fakhouri").ID,Location = "Smith 17" },
                    new OfficeAssignment { InstructorID = instructors.Single( i => i.LastName == "Harui").ID, Location = "Gowan 27" },
                    new OfficeAssignment { InstructorID = instructors.Single( i => i.LastName == "Kapoor").ID, Location = "Thompson 304" },
                };

                context.OfficeAssignments.AddRange(officeAssignments);
                context.SaveChanges();

                AddOrUpdateInstructor(context, "Chemistry", "Kapoor");
                AddOrUpdateInstructor(context, "Chemistry", "Harui");
                AddOrUpdateInstructor(context, "Microeconomics", "Zheng");
                AddOrUpdateInstructor(context, "Macroeconomics", "Zheng");
                AddOrUpdateInstructor(context, "Calculus", "Fakhouri");
                AddOrUpdateInstructor(context, "Trigonometry", "Harui");
                AddOrUpdateInstructor(context, "Composition", "Abercrombie");
                AddOrUpdateInstructor(context, "Literature", "Abercrombie");
                context.SaveChanges();

                context.Enrollments.AddRange(
                    new Enrollment { StudentID = 1, CourseID = 1050, Grade = Grade.A },
                    new Enrollment { StudentID = 1, CourseID = 4022, Grade = Grade.C },
                    new Enrollment { StudentID = 1, CourseID = 4041, Grade = Grade.B },
                    new Enrollment { StudentID = 2, CourseID = 1045, Grade = Grade.B },
                    new Enrollment { StudentID = 2, CourseID = 3141, Grade = Grade.F },
                    new Enrollment { StudentID = 2, CourseID = 2021, Grade = Grade.F },
                    new Enrollment { StudentID = 3, CourseID = 1050 },
                    new Enrollment { StudentID = 4, CourseID = 1050, },
                    new Enrollment { StudentID = 4, CourseID = 4022, Grade = Grade.F },
                    new Enrollment { StudentID = 5, CourseID = 4041, Grade = Grade.C },
                    new Enrollment { StudentID = 6, CourseID = 1045 },
                    new Enrollment { StudentID = 7, CourseID = 3141, Grade = Grade.A }
                    );
                context.SaveChanges();


            }
        }
        private static List<CourseInstructor> createCourseInstructor(List<Instructor> instructors, int courseId)
        {
            var CI = new List<CourseInstructor>();
            instructors.ForEach(i => CI.Add(new CourseInstructor { InstructorId = i.ID, CourseId = courseId }));
            return CI;
        }

        private static void AddOrUpdateInstructor(SchoolContext context, string courseTitle, string instructorName)
        {
            var crs = context.Courses.Include(c => c.CourseInstructors).ThenInclude(ci => ci.Instructor).SingleOrDefault(c => c.Title == courseTitle);
            var inst = context.Instructors.SingleOrDefault(i => i.LastName == instructorName);
            var courseInstructorIds = crs.CourseInstructors.Where(ci => ci.Instructor.LastName == instructorName);
            if (courseInstructorIds.Count() == 0)
                crs.CourseInstructors.Add(new CourseInstructor { Course=crs,CourseId=crs.CourseID,InstructorId=inst.ID,Instructor=inst});
        }


    }
}
