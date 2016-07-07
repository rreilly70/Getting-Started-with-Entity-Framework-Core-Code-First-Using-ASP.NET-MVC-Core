using System.Collections.Generic;
using ContosoUniversity.Models;
namespace ContosoUniversity.ViewModels
{
    public class InstructorIndexData
    {
        public List<Instructor> Instructors { get; set; }
        public List<Course> Courses { get; set; }
        public List<Enrollment> Enrollments { get; set; }
    }
}
