using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrollments = new HashSet<Enrollment>();
        }

        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public string Loc { get; set; } = null!;
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public string Semester { get; set; } = null!;
        public string Teacher { get; set; } = null!;
        public uint SemesterYear { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual Professor TeacherNavigation { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
