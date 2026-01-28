using CourseManagement.Models;

namespace CourseManagement;

public class DepartmentService : IDepartmentService
{
    private const int MinDepartmentNameLength = 3;
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static ServiceResult Fail(string message) => new() { IsSuccess = false, Message = message };

    public ServiceResult Create(Department department)
    {
        // BR02: Department name cannot be empty or shorter than 3 characters
        if (string.IsNullOrWhiteSpace(department.Name) || department.Name.Length < MinDepartmentNameLength)
            return Fail("Department name cannot be empty or shorter than 3 characters.");

        // BR01: Department name must be unique
        var existingDepartment = _unitOfWork.Departments.GetAll()
            .FirstOrDefault(d => d.Name != null && d.Name.Equals(department.Name, StringComparison.OrdinalIgnoreCase));
        if (existingDepartment != null)
            return Fail("Department name must be unique.");

        try
        {
            _unitOfWork.Departments.Add(department);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Department created successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2).
            return Fail($"Error creating department: {ex.Message}");
        }
    }

    public ServiceResult Update(Department department)
    {
        if (string.IsNullOrWhiteSpace(department.Name) || department.Name.Length < MinDepartmentNameLength)
            return Fail("Department name cannot be empty or shorter than 3 characters.");

        var existingDepartment = _unitOfWork.Departments.GetAll()
            .FirstOrDefault(d => d.DepartmentId != department.DepartmentId 
                && d.Name != null 
                && d.Name.Equals(department.Name, StringComparison.OrdinalIgnoreCase));
        if (existingDepartment != null)
            return Fail("Department name must be unique.");

        try
        {
            _unitOfWork.Departments.Update(department);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Department updated successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2).
            return Fail($"Error updating department: {ex.Message}");
        }
    }

    public ServiceResult Delete(int departmentId)
    {
        var department = _unitOfWork.Departments.GetById(departmentId);
        if (department == null)
            return Fail("Department not found.");

        // BR03: A department cannot be deleted if it has students
        if (_unitOfWork.Students.GetAll().Any(s => s.DepartmentId == departmentId))
            return Fail("A department cannot be deleted if it has students.");

        // BR04: A department cannot be deleted if it has courses
        if (_unitOfWork.Courses.GetAll().Any(c => c.DepartmentId == departmentId))
            return Fail("A department cannot be deleted if it has courses.");

        try
        {
            _unitOfWork.Departments.Delete(departmentId);
            _unitOfWork.Save();
            return new ServiceResult { IsSuccess = true, Message = "Department deleted successfully." };
        }
        catch (Exception ex)
        {
            // By design: map persistence errors to user-friendly message per service contract (Level 2).
            return Fail($"Error deleting department: {ex.Message}");
        }
    }

    public IEnumerable<Department> GetAll()
    {
        return _unitOfWork.Departments.GetAll();
    }
}
