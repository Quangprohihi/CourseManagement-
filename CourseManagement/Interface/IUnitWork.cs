using System;
using CourseManagement.Models;

namespace CourseManagement
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Student> Students { get; }
        IGenericRepository<Course> Courses { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<Enrollment> Enrollments { get; }
        int Save();
    }
}