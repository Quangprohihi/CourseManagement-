using CourseManagement;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace PRN222.CourseManagement.Repository.Tests
{
    [TestFixture]
    public class RepositoryTests
    {
        private CourseManagementContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<CourseManagementContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CourseManagementContext(options);
        }

        [Test]
        public void Test1_GetAll_ReturnsData()
        {
            using var context = CreateInMemoryContext();
            context.Departments.Add(new Department { Name = "IT" });
            context.SaveChanges();

            var uow = new UnitOfWork(context);
            var result = uow.Departments.GetAll();
            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public void Test2_AddStudent_IncreasesCount()
        {
            using var context = CreateInMemoryContext();
            var uow = new UnitOfWork(context);
            uow.Students.Add(new Student { StudentCode = "S1", FullName = "Test" });
            uow.Save();
            Assert.That(context.Students.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test3_GetById_ReturnsCorrectEntity()
        {
            using var context = CreateInMemoryContext();
            context.Students.Add(new Student { StudentId = 1, FullName = "Name1" });
            context.SaveChanges();

            var uow = new UnitOfWork(context);
            var result = uow.Students.GetById(1);
            Assert.That(result.FullName, Is.EqualTo("Name1"));
        }

        [Test]
        public void Test4_Delete_RemovesEntity()
        {
            using var context = CreateInMemoryContext();
            context.Students.Add(new Student { StudentId = 1, FullName = "Name1" });
            context.SaveChanges();

            var uow = new UnitOfWork(context);
            uow.Students.Delete(1);
            uow.Save();
            Assert.That(uow.Students.GetById(1), Is.Null);
        }

        [Test]
        public void Test5_UnitOfWork_SavePersistsAll()
        {
            using var context = CreateInMemoryContext();
            var uow = new UnitOfWork(context);
            uow.Departments.Add(new Department { Name = "Dept1" });
            uow.Courses.Add(new Course { Title = "Course1" });
            uow.Save();

            Assert.Multiple(() => {
                Assert.That(context.Departments.Count(), Is.EqualTo(1));
                Assert.That(context.Courses.Count(), Is.EqualTo(1));
            });
        }
    }
}