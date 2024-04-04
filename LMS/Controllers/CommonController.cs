using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query = from d in db.Departments
                        select new
                        {
                            name = d.Name,
                            subject = d.SubjectAbbreviation
                        };
            
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            // TODO: figure out how to split the json queries into matching by dept
            // maybe consider combining it into one query?
            var query = from d in db.Departments
                        select new
                        {
                            subject = d.SubjectAbbreviation,
                            dname = d.Name,
                            //nested select
                            courses = from c in d.Courses
                                      select new
                                      {
                                          number = c.Number,
                                          cname = c.Name
                                      }
                        };

            
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var query = from c in db.Courses
                        where c.Number.Equals(number)
                        && c.Name.Equals(subject)
                        select
                            from j in c.Classes
                            select new
                            {
                                season = j.Semester,
                                year = j.SemesterYear,
                                location = j.Loc,
                                start = j.Start,
                                end = j.End,
                                fname = from p in db.Professors
                                        where p.UId == j.Teacher
                                        select p.FirstName,
                                lname = from p in db.Professors
                                        where p.UId == j.Teacher
                                        select p.LastName
                            };
                        return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, 
            string category, string asgname)
        {
            // subject is in departments as subjectabbreviation
            // num is in courses as number
            // season is in classes as semester
            // year is in classes as semesteryear
            // category is in assignmentcategory as name
            // asgname is in assignments as name

            //var query = from s in db.Courses
            //            where s.DeptNavigation.SubjectAbbreviation == subject
            //            select new
            //            {

            //            };

            // use navigation go backward from assignment to category to class to course, etc. 
            var query = from a in db.Assignments
                        where a.Name.Equals(asgname) &&
                        a.CategoriesNavigation.Name.Equals(category)&&
                        a.CategoriesNavigation.Class.SemesterYear == year &&
                        a.CategoriesNavigation.Class.Semester.Equals(season) &&
                        a.CategoriesNavigation.Class.Course.Number == num &&
                        a.CategoriesNavigation.Class.Course.DeptNavigation.SubjectAbbreviation.Equals(subject)
                        select a.Contents;


            return Content(query.First());
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {            
            return Content("");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {

            var query = from a in db.Administrators
                        where a.UId.Equals(uid)
                        select new
                        {
                            fname = a.FirstName,
                            lname = a.LastName,
                            uid = a.UId,

                        };
            if (query.Any())
            {
               
                return Json(query.First());
            
            }
            else
            {
                var queryP = from p in db.Professors
                            where p.UId.Equals(uid)
                            select new
                            {
                                fname = p.FirstName,
                                lname = p.LastName,
                                uid = p.UId,
                                department = p.WorksIn

                            };
                if (queryP.Any())
                {
                    return Json(queryP.First());

                }
                else
                {
                    var queryS = from s in db.Students
                                 where s.UId.Equals(uid)
                                 select new
                                 {
                                     fname = s.FirstName,
                                     lname = s.LastName,
                                     uid = s.UId,
                                     department = s.Major

                                 };
                    if (queryS.Any())
                    {
                        Json(queryS.First());
                    }
                   
                }
            }
            


            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

