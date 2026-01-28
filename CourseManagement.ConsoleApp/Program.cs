using CourseManagement;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// 1. Cấu hình đọc file appsettings.json
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
IConfiguration config = builder.Build();

// 2. Thiết lập Dependency Injection (DI)
var services = new ServiceCollection()
    .AddDbContext<CourseManagementContext>(options =>
        options.UseSqlServer(config.GetConnectionString("DBDefault")))
    .AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
    .AddScoped<IUnitOfWork, UnitOfWork>()
    .AddScoped<IStudentService, StudentService>()
    .AddScoped<ICourseService, CourseService>()
    .AddScoped<IDepartmentService, DepartmentService>()
    .AddScoped<IEnrollmentService, EnrollmentService>()
    .BuildServiceProvider();

using (var scope = services.CreateScope())
{
    var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
    var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
    var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
    var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
    bool exit = false;

    while (!exit)
    {
        Console.WriteLine("\n=== COURSE MANAGEMENT SYSTEM ===");
        Console.WriteLine("1. List All Students");
        Console.WriteLine("2. List All Courses");
        Console.WriteLine("3. List All Departments");
        Console.WriteLine("4. Create Department");
        Console.WriteLine("5. Create Student");
        Console.WriteLine("6. Create Course");
        Console.WriteLine("7. Enroll Student into Course");
        Console.WriteLine("8. Assign Grade to Student");
        Console.WriteLine("9. Update Student Information");
        Console.WriteLine("10. Delete a Course");
        Console.WriteLine("0. Exit");
        Console.Write("Choose an option: ");
        string choice = Console.ReadLine() ?? "";

        switch (choice)
        {
            case "1":
                Console.WriteLine("\n--- Student List ---");
                foreach (var s in studentService.GetAll())
                    Console.WriteLine($"ID: {s.StudentId} | Code: {s.StudentCode} | Name: {s.FullName} | Department: {s.DepartmentId}");
                break;

            case "2":
                Console.WriteLine("\n--- Course List ---");
                foreach (var c in courseService.GetAll())
                    Console.WriteLine($"ID: {c.CourseId} | Code: {c.CourseCode} | Title: {c.Title} | Credits: {c.Credits} | Department: {c.DepartmentId}");
                break;

            case "3":
                Console.WriteLine("\n--- Department List ---");
                foreach (var d in departmentService.GetAll())
                    Console.WriteLine($"ID: {d.DepartmentId} | Name: {d.Name}");
                break;

            case "4":
                Console.Write("Enter Department Name: ");
                string deptName = Console.ReadLine() ?? "";
                var department = new Department { Name = deptName };
                var deptResult = departmentService.Create(department);
                if (deptResult.IsSuccess)
                    Console.WriteLine($"Success: {deptResult.Message}");
                else
                    Console.WriteLine($"Error: {deptResult.Message}");
                break;

            case "5":
                Console.Write("Enter Student Code: ");
                string studentCode = Console.ReadLine() ?? "";
                Console.Write("Enter Full Name: ");
                string fullName = Console.ReadLine() ?? "";
                Console.Write("Enter Email (optional): ");
                string email = Console.ReadLine() ?? "";
                Console.Write("Enter Department ID: ");
                if (int.TryParse(Console.ReadLine(), out int deptId))
                {
                    var student = new Student 
                    { 
                        StudentCode = studentCode, 
                        FullName = fullName, 
                        Email = string.IsNullOrWhiteSpace(email) ? null : email,
                        DepartmentId = deptId 
                    };
                    var studentResult = studentService.Create(student);
                    if (studentResult.IsSuccess)
                        Console.WriteLine($"Success: {studentResult.Message}");
                    else
                        Console.WriteLine($"Error: {studentResult.Message}");
                }
                else
                    Console.WriteLine("Invalid Department ID.");
                break;

            case "6":
                Console.Write("Enter Course Code: ");
                string courseCode = Console.ReadLine() ?? "";
                Console.Write("Enter Course Title: ");
                string courseTitle = Console.ReadLine() ?? "";
                Console.Write("Enter Credits (1-6): ");
                if (int.TryParse(Console.ReadLine(), out int credits))
                {
                    Console.Write("Enter Department ID: ");
                    if (int.TryParse(Console.ReadLine(), out int courseDeptId))
                    {
                        var course = new Course 
                        { 
                            CourseCode = courseCode, 
                            Title = courseTitle, 
                            Credits = credits,
                            DepartmentId = courseDeptId 
                        };
                        var courseResult = courseService.Create(course);
                        if (courseResult.IsSuccess)
                            Console.WriteLine($"Success: {courseResult.Message}");
                        else
                            Console.WriteLine($"Error: {courseResult.Message}");
                    }
                    else
                        Console.WriteLine("Invalid Department ID.");
                }
                else
                    Console.WriteLine("Invalid Credits.");
                break;

            case "7":
                Console.Write("Enter Student ID: ");
                if (int.TryParse(Console.ReadLine(), out int sId))
                {
                    Console.Write("Enter Course ID: ");
                    if (int.TryParse(Console.ReadLine(), out int cId))
                    {
                        var enrollResult = enrollmentService.Enroll(sId, cId, DateTime.Today);
                        if (enrollResult.IsSuccess)
                            Console.WriteLine($"Success: {enrollResult.Message}");
                        else
                            Console.WriteLine($"Error: {enrollResult.Message}");
                    }
                    else
                        Console.WriteLine("Invalid Course ID.");
                }
                else
                    Console.WriteLine("Invalid Student ID.");
                break;

            case "8":
                Console.Write("Enter Student ID: ");
                if (int.TryParse(Console.ReadLine(), out int gradeSId))
                {
                    Console.Write("Enter Course ID: ");
                    if (int.TryParse(Console.ReadLine(), out int gradeCId))
                    {
                        Console.Write("Enter Grade (0-10): ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal grade))
                        {
                            var gradeResult = enrollmentService.AssignGrade(gradeSId, gradeCId, grade);
                            if (gradeResult.IsSuccess)
                                Console.WriteLine($"Success: {gradeResult.Message}");
                            else
                                Console.WriteLine($"Error: {gradeResult.Message}");
                        }
                        else
                            Console.WriteLine("Invalid Grade.");
                    }
                    else
                        Console.WriteLine("Invalid Course ID.");
                }
                else
                    Console.WriteLine("Invalid Student ID.");
                break;

            case "9":
                Console.Write("Enter Student ID to update: ");
                if (int.TryParse(Console.ReadLine(), out int updateId))
                {
                    var existingStudent = studentService.GetAll().FirstOrDefault(s => s.StudentId == updateId);
                    if (existingStudent != null)
                    {
                        Console.Write("Enter New Full Name: ");
                        existingStudent.FullName = Console.ReadLine() ?? "";
                        var updateResult = studentService.Update(existingStudent);
                        if (updateResult.IsSuccess)
                            Console.WriteLine($"Success: {updateResult.Message}");
                        else
                            Console.WriteLine($"Error: {updateResult.Message}");
                    }
                    else
                        Console.WriteLine("Student not found.");
                }
                else
                    Console.WriteLine("Invalid Student ID.");
                break;

            case "10":
                Console.Write("Enter Course ID to delete: ");
                if (int.TryParse(Console.ReadLine(), out int deleteId))
                {
                    var deleteResult = courseService.Delete(deleteId);
                    if (deleteResult.IsSuccess)
                        Console.WriteLine($"Success: {deleteResult.Message}");
                    else
                        Console.WriteLine($"Error: {deleteResult.Message}");
                }
                else
                    Console.WriteLine("Invalid Course ID.");
                break;

            case "0":
                exit = true;
                break;
        }
    }
}