using CourseManagement;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace PRN222.CourseManagement.Repository.Tests;

[TestFixture]
public class EnrollmentServiceTddTests
{
    private CourseManagementContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CourseManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CourseManagementContext(options);
    }

    [Test]
    public void TC26_EnrollStudentYoungerThan18_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "Information Technology" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student
        {
            StudentCode = "S001",
            FullName = "John Doe",
            DepartmentId = department.DepartmentId,
            DateOfBirth = DateTime.Today.AddYears(-17),
            IsActive = true
        };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course
        {
            CourseCode = "CS101",
            Title = "Programming",
            DepartmentId = department.DepartmentId,
            Credits = 3,
            IsActive = true
        };
        context.Courses.Add(course);
        context.SaveChanges();

        // When
        var result = enrollmentService.Enroll(student.StudentId, course.CourseId, DateTime.Today);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("18"));
    }

    [Test]
    public void TC27_EnrollIntoCourseWithZeroCredits_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "Information Technology" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student
        {
            StudentCode = "S001",
            FullName = "John Doe",
            DepartmentId = department.DepartmentId,
            DateOfBirth = DateTime.Today.AddYears(-20),
            IsActive = true
        };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course
        {
            CourseCode = "CS101",
            Title = "Programming",
            DepartmentId = department.DepartmentId,
            Credits = 0,
            IsActive = true
        };
        context.Courses.Add(course);
        context.SaveChanges();

        // When
        var result = enrollmentService.Enroll(student.StudentId, course.CourseId, DateTime.Today);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("credit"));
    }

    [Test]
    public void TC28_EnrollIntoInactiveCourse_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "Information Technology" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student
        {
            StudentCode = "S001",
            FullName = "John Doe",
            DepartmentId = department.DepartmentId,
            DateOfBirth = DateTime.Today.AddYears(-20),
            IsActive = true
        };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course
        {
            CourseCode = "CS101",
            Title = "Programming",
            DepartmentId = department.DepartmentId,
            Credits = 3,
            IsActive = false
        };
        context.Courses.Add(course);
        context.SaveChanges();

        // When
        var result = enrollmentService.Enroll(student.StudentId, course.CourseId, DateTime.Today);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("inactive"));
    }

    [Test]
    public void TC29_EnrollInactiveStudent_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "Information Technology" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student
        {
            StudentCode = "S001",
            FullName = "John Doe",
            DepartmentId = department.DepartmentId,
            DateOfBirth = DateTime.Today.AddYears(-20),
            IsActive = false
        };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course
        {
            CourseCode = "CS101",
            Title = "Programming",
            DepartmentId = department.DepartmentId,
            Credits = 3,
            IsActive = true
        };
        context.Courses.Add(course);
        context.SaveChanges();

        // When
        var result = enrollmentService.Enroll(student.StudentId, course.CourseId, DateTime.Today);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("inactive"));
    }

    [Test]
    public void TC30_AssignGradeOutsideGradingPeriod_ShouldFail()
    {
        // Given
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var enrollmentService = new EnrollmentService(unitOfWork, context);

        var department = new Department { Name = "Information Technology" };
        context.Departments.Add(department);
        context.SaveChanges();

        var student = new Student
        {
            StudentCode = "S001",
            FullName = "John Doe",
            DepartmentId = department.DepartmentId
        };
        context.Students.Add(student);
        context.SaveChanges();

        var course = new Course
        {
            CourseCode = "CS101",
            Title = "Programming",
            DepartmentId = department.DepartmentId,
            Credits = 3
        };
        context.Courses.Add(course);
        context.SaveChanges();

        var enrollment = new Enrollment
        {
            StudentId = student.StudentId,
            CourseId = course.CourseId,
            EnrollDate = DateTime.Today.AddDays(-31),
            IsFinalized = false
        };
        context.Enrollments.Add(enrollment);
        context.SaveChanges();

        // When
        var result = enrollmentService.AssignGrade(student.StudentId, course.CourseId, 8m);

        // Then
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Message, Contains.Substring("grading period"));
    }
}
