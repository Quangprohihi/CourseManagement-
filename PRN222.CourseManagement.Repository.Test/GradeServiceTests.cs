using CourseManagement;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace PRN222.CourseManagement.Repository.Tests;

[TestFixture]
public class GradeServiceTests
{
    private CourseManagementContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CourseManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CourseManagementContext(options);
    }

    [Test]
    public void TC21_AssignGradeWithoutEnrollment_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department.DepartmentId };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3 };
        context.Courses.Add(course);
        context.SaveChanges();

        // When - No enrollment exists
        var result = enrollmentService.AssignGrade(student.StudentId, course.CourseId, 8.5m);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("enrollment exists"));
    }

    [Test]
    public void TC22_AssignInvalidGradeValue_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department.DepartmentId };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3 };
        context.Courses.Add(course);
        context.SaveChanges();

        var enrollment = new Enrollment { StudentId = student.StudentId, CourseId = course.CourseId, EnrollDate = DateTime.Today };
        context.Enrollments.Add(enrollment);
        context.SaveChanges();

        // When - Grade below 0
        var result1 = enrollmentService.AssignGrade(student.StudentId, course.CourseId, -1m);

        // When - Grade above 10
        var result2 = enrollmentService.AssignGrade(student.StudentId, course.CourseId, 11m);

        // Then
        Assert.That(result1.IsSuccess, Is.False);
        Assert.That(result1.Message, Contains.Substring("0-10"));
        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Contains.Substring("0-10"));
    }

    [Test]
    public void TC23_UpdateFinalizedGrade_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department.DepartmentId };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3 };
        context.Courses.Add(course);
        context.SaveChanges();

        var enrollment = new Enrollment 
        { 
            StudentId = student.StudentId, 
            CourseId = course.CourseId, 
            EnrollDate = DateTime.Today,
            Grade = 8.5m,
            IsFinalized = true
        };
        context.Enrollments.Add(enrollment);
        context.SaveChanges();

        // When
        var result = enrollmentService.UpdateGrade(student.StudentId, course.CourseId, 9.0m);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("finalized"));
    }
}
