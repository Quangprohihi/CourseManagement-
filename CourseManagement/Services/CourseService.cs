using CourseManagement.Models;

namespace CourseManagement;

public class CourseService : ICourseService
{
    private const int MinCredits = 1;
    private const int MaxCredits = 6;
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static ServiceResult Fail(string message) => new() { IsSuccess = false, Message = message };

    public ServiceResult Create(Course course)
    {
        // BR12: Course must belong to exactly one department
        if (!course.DepartmentId.HasValue)
            return Fail("Course must belong to exactly one department.");

        var department = _unitOfWork.Departments.GetById(course.DepartmentId.Value);
        if (department == null)
            return Fail("Department not found.");

        // BR11: CourseCode must be unique
        if (!string.IsNullOrWhiteSpace(course.CourseCode))
        {
            var existingCourse = _unitOfWork.Courses.GetAll()
                .FirstOrDefault(c => c.CourseCode != null 
                    && c.CourseCode.Equals(course.CourseCode, StringComparison.OrdinalIgnoreCase));
            if (existingCourse != null)
                return Fail("CourseCode must be unique.");
        }

        // BR13: Course credits must be between 1 and 6
        if (!course.Credits.HasValue || course.Credits.Value < MinCredits || course.Credits.Value > MaxCredits)
            return Fail("Course credits must be between 1 and 6.");

        try
        {
            _unitOfWork.Courses.Add(course);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Course created successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2).
            return Fail($"Error creating course: {ex.Message}");
        }
    }

    public ServiceResult Update(Course course)
    {
        var existingCourse = _unitOfWork.Courses.GetById(course.CourseId);
        if (existingCourse == null)
            return Fail("Course not found.");

        // BR15: A course cannot be updated if it is inactive or archived
        if (!existingCourse.IsActive || existingCourse.IsArchived)
            return Fail("A course cannot be updated if it is inactive or archived.");

        if (!course.DepartmentId.HasValue)
            return Fail("Course must belong to exactly one department.");

        var department = _unitOfWork.Departments.GetById(course.DepartmentId.Value);
        if (department == null)
            return Fail("Department not found.");

        // BR11: CourseCode must be unique (excluding current course)
        if (!string.IsNullOrWhiteSpace(course.CourseCode))
        {
            var duplicateCourse = _unitOfWork.Courses.GetAll()
                .FirstOrDefault(c => c.CourseId != course.CourseId 
                    && c.CourseCode != null 
                    && c.CourseCode.Equals(course.CourseCode, StringComparison.OrdinalIgnoreCase));
            if (duplicateCourse != null)
                return Fail("CourseCode must be unique.");
        }

        if (!course.Credits.HasValue || course.Credits.Value < MinCredits || course.Credits.Value > MaxCredits)
            return Fail("Course credits must be between 1 and 6.");

        try
        {
            _unitOfWork.Courses.Update(course);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Course updated successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2).
            return Fail($"Error updating course: {ex.Message}");
        }
    }

    public ServiceResult Delete(int courseId)
    {
        var course = _unitOfWork.Courses.GetById(courseId);
        if (course == null)
            return Fail("Course not found.");

        // BR14: A course cannot be deleted if students are enrolled
        if (_unitOfWork.Enrollments.GetAll().Any(e => e.CourseId == courseId))
            return Fail("A course cannot be deleted if students are enrolled.");

        try
        {
            _unitOfWork.Courses.Delete(courseId);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Course deleted successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2).
            return Fail($"Error deleting course: {ex.Message}");
        }
    }

    public IEnumerable<Course> GetAll()
    {
        return _unitOfWork.Courses.GetAll();
    }
}
