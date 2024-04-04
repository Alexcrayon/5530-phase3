using LMS.Controllers;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LMSControllerTests
{
    public class UnitTest1
    {
        // Common controller tests
        [Fact]
        public void CCBasic()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());

            var allDepts = ctrl.GetDepartments() as JsonResult;

            dynamic x = allDepts.Value;

            Assert.Equal(1, x.Length);
            Assert.Equal("CS", x[0].subject);
        }

        [Fact]
        public void CCTestGetUser()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());

            var user = ctrl.GetUser("u0000001") as JsonResult;
            
            
            //var allDepts = ctrl.GetDepartments() as JsonResult;
          
            dynamic x = user.Value;
                
            //Console.WriteLine(x.ToString());

            //Assert.Equal(1, x.Length);
            Assert.Equal("John", x.fname);
            Assert.Equal("Doe", x.lname);
            Assert.Equal("u0000001", x.uid);

        }

        [Fact]
        public void CCTestGetClassOfferings()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());

            var classes = ctrl.GetClassOfferings("CS", 5530 ) as JsonResult;


            //var allDepts = ctrl.GetDepartments() as JsonResult;

            dynamic x = classes.Value;

            //Console.WriteLine(x.ToString());

            Assert.Equal(2, x.Length);
            /// "season": the season part of the semester, such as "Fall"
            /// "year": the year part of the semester
            /// "location": the location of the class
            /// "start": the start time in format "hh:mm:ss"
            /// "end": the end time in format "hh:mm:ss"
            /// "fname": the first name of the professor
            /// "lname": the last name of the professor
            Assert.Equal("Spring", x[0].season);
            Assert.Equal("John", x[0].fname);
            Assert.Equal("Mary", x[1].fname);
        }

        [Fact]
        public void CCTestGetAssignmentContents()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());

            var aContent = ctrl.GetAssignmentContents("CS",5530,"Spring",2024,"Quiz","Quiz1") as ContentResult;



            dynamic x = aContent.Content;

            
            Assert.Equal("Quiz for Chapter1", x);

        }

        [Fact]
        public void CCTestGetSubmissionContents()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());

            var aContent = ctrl.GetSubmissionText("CS", 5530, "Spring", 2024, "Quiz", "Quiz1", "u0000004") as ContentResult;
            var aContent2 = ctrl.GetSubmissionText("CS", 5530, "Spring", 2024, "Quiz", "Quiz1", "u0000005") as ContentResult;


            dynamic x = aContent.Content;
            dynamic y = aContent2.Content;

            Assert.Equal("Q1 answer from John", x);
            Assert.Equal("Q1 answer from Mary", y);
        }

        //Admin controller tests
        [Fact]
        public void AGetCourses()
        {
            // An example of a simple unit test on the CommonController
            AdministratorController ctrl = new AdministratorController(MakeTinyDB());

            var courses = ctrl.GetCourses("CS") as JsonResult;

            dynamic x = courses.Value;

            Assert.Equal(5530, (int)x[0].number);
            Assert.Equal("Database Systems", x[0].name);

        }

        ///// <summary>
        ///// Make a very tiny in-memory database, containing just one department
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeTinyDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add(new Department { Name = "KSoC", SubjectAbbreviation = "CS" });

            // TODO: add more objects to the test database
            db.Administrators.Add(new Administrator { UId = "u0000001", FirstName = "John", LastName = "Doe", Dob = new DateOnly(2000, 1, 1) });
            db.Administrators.Add(new Administrator { UId = "u0000002", FirstName = "Mary", LastName = "Doe", Dob = new DateOnly(2000, 2, 2) });
            db.Administrators.Add(new Administrator { UId = "u0000003", FirstName = "Leo", LastName = "Doe", Dob = new DateOnly(2000, 3, 3) });

            db.Students.Add(new Student { UId = "u0000004", FirstName = "John", LastName = "Doe", Dob = new DateOnly(2000, 1, 1) , Major = "CS"});
            db.Students.Add(new Student { UId = "u0000005", FirstName = "Mary", LastName = "Doe", Dob = new DateOnly(2000, 2, 2), Major = "CS" });
            db.Students.Add(new Student { UId = "u0000006", FirstName = "Leo", LastName = "Doe", Dob = new DateOnly(2000, 3, 3), Major = "CS" });

            db.Professors.Add(new Professor { UId = "u0000007", FirstName = "John", LastName = "Doe", Dob = new DateOnly(2000, 1, 1), WorksIn = "CS" });
            db.Professors.Add(new Professor { UId = "u0000008", FirstName = "Mary", LastName = "Doe", Dob = new DateOnly(2000, 2, 2), WorksIn = "CS" });
            db.Professors.Add(new Professor { UId = "u0000009", FirstName = "Leo", LastName = "Doe", Dob = new DateOnly(2000, 3, 3), WorksIn = "CS" });

            
            
            db.Courses.Add(new Course { Name = "Database Systems", Number = 5530, Dept = "CS", CourseId = 1 });
            db.Classes.Add(new Class { ClassId = 1, CourseId = 1, Loc = "Online", Start = new TimeOnly(00, 00, 00), End = new TimeOnly(00, 00, 00), Semester = "Spring", SemesterYear = 2024, Teacher = "u0000007" });
            db.Classes.Add(new Class { ClassId = 2, CourseId = 1, Loc = "WEB 1000", Start = new TimeOnly(8, 00, 00), End = new TimeOnly(9, 00, 00), Semester = "Spring", SemesterYear = 2024, Teacher = "u0000008" });

            db.AssignmentCategories.Add(new AssignmentCategory {  Name ="Quiz", GradingWeight = 10, ClassId = 1, AcId = 1 });
            db.Assignments.Add(new Assignment { Name = "Quiz1", MaxPointVal = 100, Contents = "Quiz for Chapter1", Due = new DateTime(2024, 4, 3, 23, 59, 0), Categories = 1, AId = 1 });

            db.Submissions.Add(new Submission {DateTime = new DateTime(2024, 4, 3, 23, 58, 0), UId = "u0000004", AId = 1, Score = 100, Contents = "Q1 answer from John" });
            db.Submissions.Add(new Submission { DateTime = new DateTime(2024, 4, 3, 23, 58, 0), UId = "u0000005", AId = 1, Score = 100, Contents = "Q1 answer from Mary" });

            db.SaveChanges();

            return db;
        }

        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }

    }
}