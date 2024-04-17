using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /* Implemented by Chris Silva and Alex Cao for CS5530, Spring 2024 */

        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Enrollments
                        where c.UId == uid
                        select new
                        {
                            subject = c.Class.Course.Dept,
                            number = c.Class.Course.Number,
                            name = c.Class.Course.Name,
                            season = c.Class.Semester,
                            year = c.Class.SemesterYear,
                            grade = c.Grade
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var queryA = from a in db.Assignments
                         where a.CategoriesNavigation.Class.Course.Dept.Equals(subject) &&
                         a.CategoriesNavigation.Class.Course.Number == num &&
                         a.CategoriesNavigation.Class.Semester.Equals(season) &&
                         a.CategoriesNavigation.Class.SemesterYear == year
                         select a;

            var query = from a in queryA
                        join s in db.Submissions
                        on new { A = a.AId, B = uid } equals new { A = s.AId, B = s.UId }
                        into joined

                        from j in joined.DefaultIfEmpty()
                        select new
                        {
                            aname = a.Name,
                            cname = a.CategoriesNavigation.Name,
                            due = a.Due,
                            score = j != null ? (int?)j.Score : null
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var query = from a in db.Assignments
                        where a.Name.Equals(asgname) &&
                        a.CategoriesNavigation.Name.Equals(category) &&
                        a.CategoriesNavigation.Class.SemesterYear == year &&
                        a.CategoriesNavigation.Class.Semester.Equals(season) &&
                        a.CategoriesNavigation.Class.Course.Number == num &&
                        a.CategoriesNavigation.Class.Course.Dept.Equals(subject)
                        select a.AId;


            //insert into Submissions
            Submission sub = new Submission()
            {
                DateTime = DateTime.Now,
                UId = uid,
                AId = query.First(),
                Score = 0,
                Contents = contents
            };


            var querySub = from s in db.Submissions
                           where s.AId == sub.AId && s.UId.Equals(uid)
                           select s;
            
            Submission subUpdate = querySub.SingleOrDefault();

            if(subUpdate != null)
            {
                subUpdate.Contents = contents;
                subUpdate.DateTime = DateTime.Now;
            }
            else
            {
                db.Submissions.Add(sub);
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
            
            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {          
            var query = from c in db.Classes
                        where c.Course.Dept == subject &&
                        c.Course.Number == num &&
                        c.Semester == season &&
                        c.SemesterYear == year
                        select c.ClassId;

            Enrollment enrollment = new()
            {
                ClassId = query.First(),
                UId = uid,
                Grade = "--"
            };

            db.Enrollments.Add(enrollment);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
            return Json(new { success = true});
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {            
            var query = from e in db.Enrollments
                        where e.UId.Equals(uid)
                        select e.Grade;

            double GPA = 0.0;
            int numGrades = 0;
            if (query == null) 
            {
                return Json(new { gpa = 0.0 });
            }
            foreach (var c in query)
            {
                switch (c)
                {
                    case "A":
                        GPA += 4.0;
                        numGrades++;
                        break;

                    case "A-":
                        GPA += 3.7;
                        numGrades++;
                        break;

                    case "B+":
                        GPA += 3.3;
                        numGrades++;
                        break;

                    case "B":
                        GPA += 3.0;
                        numGrades++;
                        break;

                    case "B-":
                        GPA += 2.7;
                        numGrades++;
                        break;

                    case "C+":
                        GPA += 2.3;
                        numGrades++;
                        break;

                    case "C":
                        GPA += 2.0;
                        numGrades++;
                        break;

                    case "C-":
                        GPA += 1.7;
                        numGrades++;
                        break;

                    case "D+":
                        GPA += 1.3;
                        numGrades++;
                        break;

                    case "D":
                        GPA += 1.0;
                        numGrades++;
                        break;

                    case "D-":
                        GPA += 0.7;
                        numGrades++;
                        break;

                    case "E":
                        GPA += 0.0;
                        numGrades++;
                        break;

                    case "--": 
                        break;
                }
            }

            if(numGrades != 0)
            {
                GPA /= numGrades;
            }
            
            return Json(new {gpa = GPA});
        }
                
        /*******End code to modify********/

    }
}

