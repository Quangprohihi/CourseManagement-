using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using EfTransaction = Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction;

namespace CourseManagement;

public class EnrollmentService : IEnrollmentService
{
    private const int MinAgeForEnrollment = 18;
    private const int MaxCoursesPerStudent = 5;
    private const int MinCreditsToEnroll = 1;
    private const int GradingPeriodDays = 30;
    private const decimal GradeMin = 0;
    private const decimal GradeMax = 10;

    private readonly IUnitOfWork _unitOfWork;
    private readonly CourseManagementContext _context;

    public EnrollmentService(IUnitOfWork unitOfWork, CourseManagementContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    private static int GetAgeAt(DateTime? dateOfBirth, DateTime atDate)
    {
        if (!dateOfBirth.HasValue) return -1;
        var age = atDate.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > atDate.AddYears(-age)) age--;
        return age;
    }

    private static ServiceResult Fail(string message) => new() { IsSuccess = false, Message = message };

    private ServiceResult FailEnroll(EfTransaction? transaction, string message)
    {
        transaction?.Rollback();
        return Fail(message);
    }

    public ServiceResult Enroll(int studentId, int courseId, DateTime enrollDate)
    {
        // BR24: Enrollment operations must be transactional
        // Note: In-Memory database doesn't support transactions
        EfTransaction? transaction = null;
        var isInMemory = _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        
        if (!isInMemory)
        {
            transaction = _context.Database.BeginTransaction();
        }
        
        try
        {
            // BR20: Enrollment must reference existing student and course
            var student = _unitOfWork.Students.GetById(studentId);
            if (student == null)
                return FailEnroll(transaction, "Student not found.");

            var course = _unitOfWork.Courses.GetById(courseId);
            if (course == null)
                return FailEnroll(transaction, "Course not found.");

            // BR29: A student cannot be enrolled if marked as inactive
            if (!student.IsActive)
                return FailEnroll(transaction, "Student is inactive and cannot enroll.");

            // BR26: Student age must be at least 18 at enrollment time
            if (!student.DateOfBirth.HasValue)
                return FailEnroll(transaction, "Student must be at least 18 years old at enrollment. Date of birth is required.");

            if (GetAgeAt(student.DateOfBirth, enrollDate) < MinAgeForEnrollment)
                return FailEnroll(transaction, "Student must be at least 18 years old at enrollment.");

            // BR28: A student cannot enroll in inactive courses
            if (!course.IsActive)
                return FailEnroll(transaction, "Course is inactive. Enrollment is not allowed.");

            // BR27: A course must have at least 1 credit to allow enrollment
            if (!course.Credits.HasValue || course.Credits.Value < MinCreditsToEnroll)
                return FailEnroll(transaction, "Course must have at least 1 credit to allow enrollment.");

            // BR16: A student cannot enroll in the same course more than once
            var existingEnrollment = _unitOfWork.Enrollments.GetAll()
                .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);
            if (existingEnrollment != null)
                return FailEnroll(transaction, "A student cannot enroll in the same course more than once.");

            // BR17: A student can enroll in a maximum of 5 courses
            if (_unitOfWork.Enrollments.GetAll().Count(e => e.StudentId == studentId) >= MaxCoursesPerStudent)
                return FailEnroll(transaction, "A student can enroll in a maximum of 5 courses.");

            // BR18: Enrollment date cannot be in the past
            if (enrollDate < DateTime.Today)
                return FailEnroll(transaction, "Enrollment date cannot be in the past.");

            // BR19: A student can enroll only in courses of the same department
            if (!student.DepartmentId.HasValue || !course.DepartmentId.HasValue
                || student.DepartmentId.Value != course.DepartmentId.Value)
                return FailEnroll(transaction, "A student can enroll only in courses of the same department.");

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollDate = enrollDate,
                Grade = null,
                IsFinalized = false
            };

            _unitOfWork.Enrollments.Add(enrollment);
            _unitOfWork.Save();
            if (transaction != null) transaction.Commit();

            return new ServiceResult
            {
                IsSuccess = true,
                Message = "Student enrolled successfully."
            };
        }
        catch (Exception ex)
        {
            // By design: map persistence/transaction errors to user-friendly message per service contract (Level 2).
            if (transaction != null) transaction.Rollback();
            return Fail($"Error enrolling student: {ex.Message}");
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    public ServiceResult AssignGrade(int studentId, int courseId, decimal grade)
    {
        var enrollment = _unitOfWork.Enrollments.GetAll()
            .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);
        if (enrollment == null)
            return Fail("Grade can be assigned only after enrollment exists.");

        if (grade < GradeMin || grade > GradeMax)
            return Fail("Grade value must be within a valid range (0-10).");

        if (enrollment.IsFinalized)
            return Fail("Grade cannot be updated once it is finalized.");

        if (!IsWithinGradingPeriod(enrollment.EnrollDate))
            return Fail("Grade can be assigned only within the grading period (30 days from enrollment).");

        try
        {
            enrollment.Grade = grade;
            _unitOfWork.Enrollments.Update(enrollment);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Grade assigned successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2).
            return Fail($"Error assigning grade: {ex.Message}");
        }
    }

    private static bool IsWithinGradingPeriod(DateTime? enrollDate)
    {
        if (!enrollDate.HasValue) return false;
        return (DateTime.Today - enrollDate.Value.Date).TotalDays <= GradingPeriodDays;
    }

    public ServiceResult UpdateGrade(int studentId, int courseId, decimal grade)
    {
        var enrollment = _unitOfWork.Enrollments.GetAll()
            .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);
        if (enrollment == null)
            return Fail("Grade can be assigned only after enrollment exists.");
        if (enrollment.IsFinalized)
            return Fail("Grade cannot be updated once it is finalized.");
        if (grade < GradeMin || grade > GradeMax)
            return Fail("Grade value must be within a valid range (0-10).");
        if (!IsWithinGradingPeriod(enrollment.EnrollDate))
            return Fail("Grade can be assigned only within the grading period (30 days from enrollment).");

        try
        {
            enrollment.Grade = grade;
            _unitOfWork.Enrollments.Update(enrollment);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Grade updated successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2).
            return Fail($"Error updating grade: {ex.Message}");
        }
    }

    public IEnumerable<Enrollment> GetAll()
    {
        return _unitOfWork.Enrollments.GetAll();
    }
}
