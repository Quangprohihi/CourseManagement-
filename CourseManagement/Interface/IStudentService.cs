using CourseManagement.Models;

namespace CourseManagement;

public interface IStudentService
{
    ServiceResult Create(Student student);
    ServiceResult Update(Student student);
    ServiceResult Delete(int studentId);
    IEnumerable<Student> GetAll();
}
