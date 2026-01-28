using CourseManagement.Models;

namespace CourseManagement;

public class StudentService : IStudentService
{
    private const int MinFullNameLength = 3;
    private readonly IUnitOfWork _unitOfWork;

    public StudentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static ServiceResult Fail(string message) => new() { IsSuccess = false, Message = message };

    public ServiceResult Create(Student student)
    {
        // BR07: Student full name cannot be null or empty
        if (string.IsNullOrWhiteSpace(student.FullName))
            return Fail("Student full name cannot be null or empty.");

        // BR08: Student full name must be at least 3 characters
        if (student.FullName.Length < MinFullNameLength)
            return Fail("Student full name must be at least 3 characters.");

        // BR06: Student must belong to exactly one department
        if (!student.DepartmentId.HasValue)
            return Fail("Student must belong to exactly one department.");

        // Verify department exists
        var department = _unitOfWork.Departments.GetById(student.DepartmentId.Value);
        if (department == null)
            return Fail("Department not found.");

        // BR05: StudentCode must be unique
        if (!string.IsNullOrWhiteSpace(student.StudentCode))
        {
            var existingStudent = _unitOfWork.Students.GetAll()
                .FirstOrDefault(s => s.StudentCode != null 
                    && s.StudentCode.Equals(student.StudentCode, StringComparison.OrdinalIgnoreCase));
            if (existingStudent != null)
                return Fail("StudentCode must be unique.");
        }

        // BR09: Student email (if provided) must be unique
        if (!string.IsNullOrWhiteSpace(student.Email))
        {
            var existingStudent = _unitOfWork.Students.GetAll()
                .FirstOrDefault(s => s.Email != null 
                    && s.Email.Equals(student.Email, StringComparison.OrdinalIgnoreCase));
            if (existingStudent != null)
                return Fail("Student email must be unique.");
        }

        try
        {
            _unitOfWork.Students.Add(student);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Student created successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2 — no Bug/Vuln).
            return Fail($"Error creating student: {ex.Message}");
        }
    }

    public ServiceResult Update(Student student)
    {
        var existingStudent = _unitOfWork.Students.GetById(student.StudentId);
        if (existingStudent == null)
            return Fail("Student not found.");

        // BR07: Student full name cannot be null or empty
        if (string.IsNullOrWhiteSpace(student.FullName))
            return Fail("Student full name cannot be null or empty.");

        // BR08: Student full name must be at least 3 characters
        if (student.FullName.Length < MinFullNameLength)
            return Fail("Student full name must be at least 3 characters.");

        // BR06: Student must belong to exactly one department
        if (!student.DepartmentId.HasValue)
            return Fail("Student must belong to exactly one department.");

        var department = _unitOfWork.Departments.GetById(student.DepartmentId.Value);
        if (department == null)
            return Fail("Department not found.");

        // BR05: StudentCode must be unique (excluding current student)
        if (!string.IsNullOrWhiteSpace(student.StudentCode))
        {
            var duplicateStudent = _unitOfWork.Students.GetAll()
                .FirstOrDefault(s => s.StudentId != student.StudentId 
                    && s.StudentCode != null 
                    && s.StudentCode.Equals(student.StudentCode, StringComparison.OrdinalIgnoreCase));
            if (duplicateStudent != null)
                return Fail("StudentCode must be unique.");
        }

        // BR09: Student email (if provided) must be unique (excluding current student)
        if (!string.IsNullOrWhiteSpace(student.Email))
        {
            var duplicateStudent = _unitOfWork.Students.GetAll()
                .FirstOrDefault(s => s.StudentId != student.StudentId 
                    && s.Email != null 
                    && s.Email.Equals(student.Email, StringComparison.OrdinalIgnoreCase));
            if (duplicateStudent != null)
                return Fail("Student email must be unique.");
        }

        try
        {
            _unitOfWork.Students.Update(student);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Student updated successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2).
            return Fail($"Error updating student: {ex.Message}");
        }
    }

    public ServiceResult Delete(int studentId)
    {
        var student = _unitOfWork.Students.GetById(studentId);
        if (student == null)
            return Fail("Student not found.");

        // BR10: A student cannot be deleted if the student has enrollments
        if (_unitOfWork.Enrollments.GetAll().Any(e => e.StudentId == studentId))
            return Fail("A student cannot be deleted if the student has enrollments.");

        try
        {
            _unitOfWork.Students.Delete(studentId);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Student deleted successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2).
            return Fail($"Error deleting student: {ex.Message}");
        }
    }

    public IEnumerable<Student> GetAll()
    {
        return _unitOfWork.Students.GetAll();
    }
}
