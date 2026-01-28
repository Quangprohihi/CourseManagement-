using System;
using System.Collections.Generic;

namespace CourseManagement.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string? CourseCode { get; set; }

    public string? Title { get; set; }

    public int? Credits { get; set; }

    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsArchived { get; set; } = false;

    public virtual Department? Department { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
