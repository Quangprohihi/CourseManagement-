using CourseManagement;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace PRN222.CourseManagement.Repository.Tests;

[TestFixture]
public class CourseServiceTests
{
    private CourseManagementContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CourseManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CourseManagementContext(options);
    }

    [Test]
    public void TC11_CreateCourseWithDuplicateCourseCode_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var courseService = new CourseService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var existingCourse = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3 };
        context.Courses.Add(existingCourse);
        context.SaveChanges();

        var duplicateCourse = new Course { CourseCode = "CS101", Title = "Advanced Programming", DepartmentId = department.DepartmentId, Credits = 3 };

        // When
        var result = courseService.Create(duplicateCourse);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("CourseCode"));
    }

    [Test]
    public void TC12_CreateCourseWithoutDepartment_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var courseService = new CourseService(unitOfWork);

        var course = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = null, Credits = 3 };

        // When
        var result = courseService.Create(course);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("department"));
    }

    [Test]
    public void TC13_CreateCourseWithInvalidCredits_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var courseService = new CourseService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var courseWithLowCredits = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 0 };
        var courseWithHighCredits = new Course { CourseCode = "CS102", Title = "Advanced Programming", DepartmentId = department.DepartmentId, Credits = 7 };

        // When
        var result1 = courseService.Create(courseWithLowCredits);
        var result2 = courseService.Create(courseWithHighCredits);

        // Then
        Assert.That(result1.IsSuccess, Is.False);
        Assert.That(result1.Message, Contains.Substring("1 and 6"));
        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Contains.Substring("1 and 6"));
    }

    [Test]
    public void TC14_DeleteCourseWithEnrollments_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var courseService = new CourseService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department.DepartmentId };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3 };
        context.Courses.Add(course);
        context.SaveChanges();

        var enrollment = new Enrollment { StudentId = student.StudentId, CourseId = course.CourseId, EnrollDate = DateTime.Now };
        context.Enrollments.Add(enrollment);
        context.SaveChanges();

        // When
        var result = courseService.Delete(course.CourseId);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("enrolled"));
    }

    [Test]
    public void TC15_UpdateInactiveCourse_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var courseService = new CourseService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var inactiveCourse = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3, IsActive = false };
        context.Courses.Add(inactiveCourse);
        context.SaveChanges();

        inactiveCourse.Title = "Updated Programming";

        // When
        var result = courseService.Update(inactiveCourse);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("inactive or archived"));
    }

    [Test]
    public void TC15_UpdateArchivedCourse_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var courseService = new CourseService(unitOfWork);

        var department = new Department { Name = "IT" };
        context.Departments.Add(department);
        context.SaveChanges();

        var archivedCourse = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3, IsActive = true, IsArchived = true };
        context.Courses.Add(archivedCourse);
        context.SaveChanges();

        archivedCourse.Title = "Updated Programming";

        // When
        var result = courseService.Update(archivedCourse);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("inactive or archived"));
    }
}
