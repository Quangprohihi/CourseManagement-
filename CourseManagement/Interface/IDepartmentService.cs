using CourseManagement.Models;

namespace CourseManagement;

public interface IDepartmentService
{
    ServiceResult Create(Department department);
    ServiceResult Update(Department department);
    ServiceResult Delete(int departmentId);
    IEnumerable<Department> GetAll();
}
