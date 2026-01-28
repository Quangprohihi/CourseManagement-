using CourseManagement;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace PRN222.CourseManagement.Repository.Tests;

[TestFixture]
public class EnrollmentServiceTests
{
    private CourseManagementContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CourseManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CourseManagementContext(options);
    }

    [Test]
    public void TC16_EnrollSameStudentIntoSameCourseTwice_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "Information Technology" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department.DepartmentId, DateOfBirth = DateTime.Today.AddYears(-20), IsActive = true };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3, IsActive = true };
        context.Courses.Add(course);
        context.SaveChanges();

        var enrollment = new Enrollment { StudentId = student.StudentId, CourseId = course.CourseId, EnrollDate = DateTime.Today };
        context.Enrollments.Add(enrollment);
        context.SaveChanges();

        // When
        var result = enrollmentService.Enroll(student.StudentId, course.CourseId, DateTime.Today);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("more than once"));
    }

    [Test]
    public void TC17_EnrollStudentExceedingMaxCourses_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "Information Technology" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department.DepartmentId, DateOfBirth = DateTime.Today.AddYears(-20), IsActive = true };
        context.Students.Add(student);
        context.SaveChanges();

        // Create 5 courses and enrollments
        for (int i = 1; i <= 5; i++)
        {
            var course = new Course { CourseCode = $"CS10{i}", Title = $"Course {i}", DepartmentId = department.DepartmentId, Credits = 3, IsActive = true };
            context.Courses.Add(course);
            context.SaveChanges();

            var enrollment = new Enrollment { StudentId = student.StudentId, CourseId = course.CourseId, EnrollDate = DateTime.Today };
            context.Enrollments.Add(enrollment);
            context.SaveChanges();
        }

        // Create 6th course
        var course6 = new Course { CourseCode = "CS106", Title = "Course 6", DepartmentId = department.DepartmentId, Credits = 3, IsActive = true };
        context.Courses.Add(course6);
        context.SaveChanges();

        // When
        var result = enrollmentService.Enroll(student.StudentId, course6.CourseId, DateTime.Today);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("maximum of 5"));
    }

    [Test]
    public void TC18_EnrollStudentWithPastDate_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "Information Technology" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department.DepartmentId, DateOfBirth = DateTime.Today.AddYears(-20), IsActive = true };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3, IsActive = true };
        context.Courses.Add(course);
        context.SaveChanges();

        var pastDate = DateTime.Today.AddDays(-1);

        // When
        var result = enrollmentService.Enroll(student.StudentId, course.CourseId, pastDate);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("past"));
    }

    [Test]
    public void TC19_EnrollStudentAcrossDepartments_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department1 = new Department { Name = "Information Technology" };
        var department2 = new Department { Name = "Business Administration" };
        context.Departments.Add(department1);
        context.Departments.Add(department2);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department1.DepartmentId, DateOfBirth = DateTime.Today.AddYears(-20), IsActive = true };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department2.DepartmentId, Credits = 3, IsActive = true };
        context.Courses.Add(course);
        context.SaveChanges();

        // When
        var result = enrollmentService.Enroll(student.StudentId, course.CourseId, DateTime.Today);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("same department"));
    }

    [Test]
    public void TC20_EnrollWithNonExistingStudentOrCourse_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "Information Technology" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student { StudentCode = "S001", FullName = "John Doe", DepartmentId = department.DepartmentId, DateOfBirth = DateTime.Today.AddYears(-20), IsActive = true };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course { CourseCode = "CS101", Title = "Programming", DepartmentId = department.DepartmentId, Credits = 3, IsActive = true };
        context.Courses.Add(course);
        context.SaveChanges();

        // When - Non-existing student
        var result1 = enrollmentService.Enroll(999, course.CourseId, DateTime.Today);

        // When - Non-existing course
        var result2 = enrollmentService.Enroll(student.StudentId, 999, DateTime.Today);

        // Then
        Assert.That(result1.IsSuccess, Is.False);
        Assert.That(result1.Message, Contains.Substring("Student not found"));
        Assert.That(result2.IsSuccess, Is.False);
        Assert.That(result2.Message, Contains.Substring("Course not found"));
    }
}
