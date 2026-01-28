using CourseManagement;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace PRN222.CourseManagement.Repository.Tests;

[TestFixture]
public class DepartmentServiceTests
{
    private CourseManagementContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CourseManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CourseManagementContext(options);
    }

    [Test]
    public void TC01_CreateDepartmentWithDuplicateName_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var departmentService = new DepartmentService(unitOfWork);

        var existingDepartment = new Department { Name = "Information Technology" };
        context.Departments.Add(existingDepartment);
        context.SaveChanges();

        var duplicateDepartment = new Department { Name = "Information Technology" };

        // When
        var result = departmentService.Create(duplicateDepartment);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("unique"));
    }

    [Test]
    public void TC02_CreateDepartmentWithShortName_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var departmentService = new DepartmentService(unitOfWork);

        var department = new Department { Name = "AB" }; // Less than 3 characters

        // When
        var result = departmentService.Create(department);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("3 characters"));
    }

    [Test]
    public void TC03_DeleteDepartmentWithStudents_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var departmentService = new DepartmentService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { FullName = "John Doe", DepartmentId = department.DepartmentId };
        context.Students.Add(student);
        context.SaveChanges();

        // When
        var result = departmentService.Delete(department.DepartmentId);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("students"));
    }

    [Test]
    public void TC04_DeleteDepartmentWithCourses_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var departmentService = new DepartmentService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var course = new Course { Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3 };
        context.Courses.Add(course);
        context.SaveChanges();

        // When
        var result = departmentService.Delete(department.DepartmentId);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("courses"));
    }
}
