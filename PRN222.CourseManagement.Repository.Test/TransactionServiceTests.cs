using CourseManagement;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace PRN222.CourseManagement.Repository.Tests;

[TestFixture]
public class TransactionServiceTests
{
    private CourseManagementContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CourseManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CourseManagementContext(options);
    }

    [Test]
    public void TC24_EnrollmentFails_NoDataPersisted_TransactionRollback()
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

        // Create 5 enrollments to reach the limit
        for (int i = 1; i <= 5; i++)
        {
            var courseItem = new Course { CourseCode = $"CS10{i}", Title = $"Course {i}", DepartmentId = department.DepartmentId, Credits = 3 };
            context.Courses.Add(courseItem);
            context.SaveChanges();

            var enrollment = new Enrollment { StudentId = student.StudentId, CourseId = courseItem.CourseId, EnrollDate = DateTime.Today };
            context.Enrollments.Add(enrollment);
            context.SaveChanges();
        }

        var initialEnrollmentCount = context.Enrollments.Count();

        // When - Try to enroll in 6th course (should fail due to BR17)
        var result = enrollmentService.Enroll(student.StudentId, course.CourseId, DateTime.Today);

        // Then - Transaction should rollback, no new enrollment should be persisted
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(context.Enrollments.Count(), Is.EqualTo(initialEnrollmentCount));
    }

    [Test]
    public void TC25_ServiceReturnsFailureResultInsteadOfException()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);
        var studentService = new StudentService(unitOfWork);
        var courseService = new CourseService(unitOfWork);
        var departmentService = new DepartmentService(unitOfWork);

        // When - Various invalid operations that would normally throw exceptions
        ServiceResult result1 = null!;
        ServiceResult result2 = null!;
        ServiceResult result3 = null!;
        ServiceResult result4 = null!;

        // Then - All should return ServiceResult with IsSuccess = false, no exceptions thrown
        Assert.DoesNotThrow(() => 
        {
            result1 = enrollmentService.Enroll(999, 999, DateTime.Today); // Non-existing student and course
        });
        Assert.That(result1.IsSuccess, Is.False);
        Assert.That(result1.Message, Is.Not.Empty);

        Assert.DoesNotThrow(() => 
        {
            result2 = studentService.Create(new Student { FullName = "AB", DepartmentId = 999 }); // Invalid student
        });
        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Is.Not.Empty);

        Assert.DoesNotThrow(() => 
        {
            result3 = courseService.Create(new Course { CourseCode = "CS101", Title = "Test", DepartmentId = 999, Credits = 0 }); // Invalid course
        });
        Assert.That(result3.IsSuccess, Is.False);
        Assert.That(result3.Message, Is.Not.Empty);

        Assert.DoesNotThrow(() => 
        {
            result4 = departmentService.Create(new Department { Name = "AB" }); // Invalid department
        });
        Assert.That(result4.IsSuccess, Is.False);
        Assert.That(result4.Message, Is.Not.Empty);
    }
}
