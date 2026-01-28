using CourseManagement.Models;

namespace CourseManagement;

public interface IEnrollmentService
{
    ServiceResult Enroll(int studentId, int courseId, DateTime enrollDate);
    ServiceResult AssignGrade(int studentId, int courseId, decimal grade);
    ServiceResult UpdateGrade(int studentId, int courseId, decimal grade);
    IEnumerable<Enrollment> GetAll();
}
