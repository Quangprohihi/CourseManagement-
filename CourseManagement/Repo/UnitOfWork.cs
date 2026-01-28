using CourseManagement.Models;

namespace CourseManagement
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CourseManagementContext _context;
        public IGenericRepository<Student> Students { get; }
        public IGenericRepository<Course> Courses { get; }
        public IGenericRepository<Department> Departments { get; }
        public IGenericRepository<Enrollment> Enrollments { get; }

        public UnitOfWork(CourseManagementContext context)
        {
            _context = context;
            Students = new GenericRepository<Student>(_context);
            Courses = new GenericRepository<Course>(_context);
            Departments = new GenericRepository<Department>(_context);
            Enrollments = new GenericRepository<Enrollment>(_context);
        }

        public int Save() => _context.SaveChanges(); // Hoặc _context.SaveChanges() tùy thuộc vào Scaffold [cite: 84, 89]
        public void Dispose() => _context.Dispose();
    }
}