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
        // Uncomment the methods below after scaffolding
        // (they won't compile until then)

        [Fact]
        public void Test1()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());

            var allDepts = ctrl.GetDepartments() as JsonResult;

            dynamic x = allDepts.Value;

            Assert.Equal(1, x.Length);
            Assert.Equal("CS", x[0].subject);
        }

        [Fact]
        public void TestGetUser()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());

            var user = ctrl.GetUser("u0000001") as JsonResult;
            
            //var allDepts = ctrl.GetDepartments() as JsonResult;
          
            dynamic x = user.Value;
                
            Console.WriteLine(x.ToString());
           
            Assert.Equal("u0000001", x.uid);
            
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