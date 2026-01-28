using CourseManagement;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace PRN222.CourseManagement.Repository.Tests;

[TestFixture]
public class StudentServiceTests
{
    private CourseManagementContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CourseManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CourseManagementContext(options);
    }

    [Test]
    public void TC05_CreateStudentWithDuplicateStudentCode_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var studentService = new StudentService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var existingStudent = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department.DepartmentId };
        context.Students.Add(existingStudent);
        context.SaveChanges();

        var duplicateStudent = new Student { StudentCode = "S001", FullName = "Jane Doe", DepartmentId = department.DepartmentId };

        // When
        var result = studentService.Create(duplicateStudent);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("StudentCode"));
    }

    [Test]
    public void TC06_CreateStudentWithoutDepartment_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var studentService = new StudentService(unitOfWork);

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = null };

        // When
        var result = studentService.Create(student);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("department"));
    }

    [Test]
    public void TC07_CreateStudentWithEmptyFullName_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var studentService = new StudentService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "", DepartmentId = department.DepartmentId };

        // When
        var result = studentService.Create(student);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("null or empty"));
    }

    [Test]
    public void TC08_CreateStudentWithShortName_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var studentService = new StudentService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "AB", DepartmentId = department.DepartmentId }; // Less than 3 characters

        // When
        var result = studentService.Create(student);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("3 characters"));
    }

    [Test]
    public void TC09_CreateStudentWithDuplicateEmail_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var studentService = new StudentService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var existingStudent = new Student { StudentCode = "S001", FullName = "John Doe", Email = "john@example.com", DepartmentId = department.DepartmentId };
        context.Students.Add(existingStudent);
        context.SaveChanges();

        var duplicateStudent = new Student { StudentCode = "S002", FullName = "Jane Doe", Email = "john@example.com", DepartmentId = department.DepartmentId };

        // When
        var result = studentService.Create(duplicateStudent);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("email"));
    }

    [Test]
    public void TC10_DeleteStudentWithEnrollments_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var studentService = new StudentService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department.DepartmentId };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course { Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3 };
        context.Courses.Add(course);
        context.SaveChanges();

        var enrollment = new Enrollment { StudentId = student.StudentId, CourseId = course.CourseId, EnrollDate = DateTime.Now };
        context.Enrollments.Add(enrollment);
        context.SaveChanges();

        // When
        var result = studentService.Delete(student.StudentId);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("enrollments"));
    }
}
