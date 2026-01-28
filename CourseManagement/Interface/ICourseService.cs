using CourseManagement.Models;

namespace CourseManagement;

public interface ICourseService
{
    ServiceResult Create(Course course);
    ServiceResult Update(Course course);
    ServiceResult Delete(int courseId);
    IEnumerable<Course> GetAll();
}
