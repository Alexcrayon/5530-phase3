using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LMS.Models.LMSModels
{
    public partial class LMSContext : DbContext
    {
        public LMSContext()
        {
        }

        public LMSContext(DbContextOptions<LMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Administrator> Administrators { get; set; } = null!;
        public virtual DbSet<Assignment> Assignments { get; set; } = null!;
        public virtual DbSet<AssignmentCategory> AssignmentCategories { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Enrollment> Enrollments { get; set; } = null!;
        public virtual DbSet<Professor> Professors { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<Submission> Submissions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("name=LMS:LMSConnectionString", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.11.3-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");

            modelBuilder.Entity<Administrator>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(255);

                entity.Property(e => e.LastName).HasMaxLength(255);
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.AId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => new { e.Categories, e.Name }, "Categories")
                    .IsUnique();

                entity.Property(e => e.AId)
                    .HasColumnType("int(11)")
                    .HasColumnName("aID");

                entity.Property(e => e.Categories).HasColumnType("int(11)");

                entity.Property(e => e.Contents).HasMaxLength(8192);

                entity.Property(e => e.Due).HasColumnType("datetime");

                entity.Property(e => e.MaxPointVal).HasColumnType("int(10) unsigned");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.CategoriesNavigation)
                    .WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.Categories)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Assignments_ibfk_1");
            });

            modelBuilder.Entity<AssignmentCategory>(entity =>
            {
                entity.HasKey(e => e.AcId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => new { e.ClassId, e.Name }, "ClassID")
                    .IsUnique();

                entity.Property(e => e.AcId)
                    .HasColumnType("int(11)")
                    .HasColumnName("acID");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("ClassID");

                entity.Property(e => e.GradingWeight).HasColumnType("int(10) unsigned");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.AssignmentCategories)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("AssignmentCategories_ibfk_1");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => e.Teacher, "Teacher");

                entity.HasIndex(e => new { e.CourseId, e.Semester, e.SemesterYear }, "unique_semester")
                    .IsUnique();

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("ClassID");

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(11)")
                    .HasColumnName("CourseID");

                entity.Property(e => e.End).HasColumnType("time");

                entity.Property(e => e.Loc).HasMaxLength(100);

                entity.Property(e => e.SemesterYear).HasColumnType("int(10) unsigned");

                entity.Property(e => e.Start).HasColumnType("time");

                entity.Property(e => e.Teacher)
                    .HasMaxLength(8)
                    .IsFixedLength();

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_3");

                entity.HasOne(d => d.TeacherNavigation)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.Teacher)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_2");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => new { e.Dept, e.Number }, "Dept")
                    .IsUnique();

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(11)")
                    .HasColumnName("CourseID");

                entity.Property(e => e.Dept).HasMaxLength(4);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Number).HasColumnType("int(4) unsigned");

                entity.HasOne(d => d.DeptNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.Dept)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Courses_ibfk_1");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.SubjectAbbreviation)
                    .HasName("PRIMARY");

                entity.Property(e => e.SubjectAbbreviation).HasMaxLength(4);

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => new { e.UId, e.ClassId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasIndex(e => e.ClassId, "ClassID");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("ClassID");

                entity.Property(e => e.Grade).HasMaxLength(2);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollments_ibfk_2");

                entity.HasOne(d => d.UIdNavigation)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.UId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollments_ibfk_1");
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.WorksIn, "WorksIn");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(255);

                entity.Property(e => e.LastName).HasMaxLength(255);

                entity.Property(e => e.WorksIn).HasMaxLength(4);

                entity.HasOne(d => d.WorksInNavigation)
                    .WithMany(p => p.Professors)
                    .HasForeignKey(d => d.WorksIn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Professors_ibfk_1");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Major, "Major");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(255);

                entity.Property(e => e.LastName).HasMaxLength(255);

                entity.Property(e => e.Major).HasMaxLength(4);

                entity.HasOne(d => d.MajorNavigation)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.Major)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Students_ibfk_1");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => new { e.UId, e.AId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("Submission");

                entity.HasIndex(e => e.AId, "aID");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.AId)
                    .HasColumnType("int(11)")
                    .HasColumnName("aID");

                entity.Property(e => e.Contents).HasMaxLength(8192);

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.Score).HasColumnType("int(10) unsigned");

                entity.HasOne(d => d.AIdNavigation)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.AId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submission_ibfk_2");

                entity.HasOne(d => d.UIdNavigation)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.UId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submission_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
