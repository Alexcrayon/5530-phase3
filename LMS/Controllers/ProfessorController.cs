using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /* Implemented by Chris Silva and Alex Cao for CS5530, Spring 2024 */
        

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student'c grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var queryClasses = from c in db.Classes
                               where c.Course.Dept.Equals(subject) &&
                               c.Course.Number == num &&
                               c.Semester == season &&
                               c.SemesterYear == year
                               select c;

            var query = from c in queryClasses
                        join e in db.Enrollments
                        on c.ClassId equals e.ClassId
                        join s in db.Students
                        on e.UId equals s.UId
                        select new
                        {
                            fname = s.FirstName,
                            lname = s.LastName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = e.Grade
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {

            var query = from c in db.Classes
                        where c.Course.Dept.Equals(subject) &&
                        c.Course.Number == num &&
                        c.Semester.Equals(season) &&
                        c.SemesterYear == year
                        select c;

         
            if (category != null)
            {
                var queryA = from q in query
                             from ac in q.AssignmentCategories
                             where ac.Name.Equals(category)
                             from a in ac.Assignments
                             select new
                             {
                                 aname = a.Name,
                                 cname = ac.Name,
                                 due = a.Due,
                                 submissions = a.Submissions.Count
                             };
                return Json(queryA.ToArray());

            }
            else
            {
                var queryAll = from q in query
                               from ac in q.AssignmentCategories
                               from a in ac.Assignments
                               select new
                               {
                                   aname = a.Name,
                                   cname = ac.Name,
                                   due = a.Due,
                                   submissions = a.Submissions.Count
                               };

                return Json(queryAll.ToArray());
            }

        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from c in db.Classes
                        where c.Course.Dept.Equals(subject) &&
                        c.Course.Number == num &&
                        c.Semester.Equals(season) &&
                        c.SemesterYear == year
                        from ac in c.AssignmentCategories
                        select new
                        {
                            name = ac.Name,
                            weight = ac.GradingWeight
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            //insert new category, false if existed
            //add foreign key in class.assignmentCategories

            var query = from c in db.Classes
                        where c.Course.Dept.Equals(subject) &&
                        c.Course.Number == num &&
                        c.Semester.Equals(season) &&
                        c.SemesterYear == year
                        select c;

            Class tempClass = query.SingleOrDefault();
            if (tempClass == null)
            {
                return Json(new { success = false });
            }


            AssignmentCategory newAc = new()
            {
                Name = category,
                GradingWeight = (uint)catweight,
                ClassId = tempClass.ClassId
            };


            tempClass.AssignmentCategories.Add(newAc);
              

            db.AssignmentCategories.Add(newAc);

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
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var query = from ac in db.AssignmentCategories
                        where ac.Class.Course.Dept.Equals(subject) &&
                        ac.Class.Course.Number == num &&
                        ac.Class.Semester.Equals(season) &&
                        ac.Class.SemesterYear == year &&
                        ac.Name.Equals(category)
                        select ac;
            AssignmentCategory cate = query.SingleOrDefault();
            if (cate == null)
            {
                return Json(new { success = false });
            }

            Assignment newA = new()
            {
                Name = asgname,
                MaxPointVal = (uint)asgpoints,
                Contents = asgcontents,
                Due = asgdue,
                Categories = cate.AcId
            };

            
            cate.Assignments.Add(newA);

            db.Assignments.Add(newA);


            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }

            AutoGrading(query.First().ClassId);

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            
            var query = from a in db.Assignments
                        where a.CategoriesNavigation.Class.Course.Dept.Equals(subject) &&
                        a.CategoriesNavigation.Class.Course.Number == num &&
                        a.CategoriesNavigation.Class.Semester.Equals(season) &&
                        a.CategoriesNavigation.Class.SemesterYear == year &&
                        a.CategoriesNavigation.Name.Equals(category) &&
                        a.Name.Equals(asgname)
                        select a;

            var querySub = from a in query
                           join s in db.Submissions
                           on a.AId equals s.AId
                           join st in db.Students
                           on s.UId equals st.UId
                           select new
                           {
                               fname = st.FirstName,
                               lname = st.LastName,
                               uid = st.UId,
                               time = s.DateTime,
                               score = s.Score
                           };


            return Json(querySub.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who'c submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {

            var query = from a in db.Assignments
                        where a.Name.Equals(asgname) &&
                        a.CategoriesNavigation.Name.Equals(category) &&
                        a.CategoriesNavigation.Class.SemesterYear == year &&
                        a.CategoriesNavigation.Class.Semester.Equals(season) &&
                        a.CategoriesNavigation.Class.Course.Number == num &&
                        a.CategoriesNavigation.Class.Course.Dept.Equals(subject)
                        select new {
                            aid = a.AId,
                            classId = a.CategoriesNavigation.ClassId
                            
                        };


            var querySub = from s in db.Submissions
                           where s.AId == query.First().aid && s.UId.Equals(uid)
                           select s;

            Submission subUpdate = querySub.SingleOrDefault();

            if (subUpdate != null)
            {
                subUpdate.Score = (uint)score;
            }
            else
            {
                return Json(new { success = false });
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }

            AutoGrading(query.First().classId);

            return Json(new { success = true });

        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor'c uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {

            var query = from c in db.Classes
                        where c.Teacher.Equals(uid)
                        select new
                        {
                            subject = c.Course.Dept,
                            number = c.Course.Number,
                            name = c.Course.Name,
                            season = c.Semester,
                            year = c.SemesterYear
                        };
            return Json(query.ToArray());
        }



        /// <summary>
        /// AutoGrading when CreateAssignment or GradeSubmission get called
        /// this is for every student in the input class
        /// </summary>
        /// <param name="ClassID"></param>
        public void AutoGrading(int ClassID)
        {
            // all students in this class
            var queryUid = from e in db.Enrollments
                           where e.ClassId == ClassID
                           select e.UId;

            // set scores to null with a left join on uid to aid so non submissions are scored as a 0
            var querySubmissions = from st in queryUid
                                   join s in db.Submissions
                                   on st equals s.UId
                                   into studentSubmissions
                                   from sub in studentSubmissions.DefaultIfEmpty()
                                       //AIdNavigation.CategoriesNavigation.ClassId == ClassID
                                   select new
                                   {
                                       uid = st,
                                       aid = sub.AId,
                                       score = sub.Score,
                                   };


            // skips calculating categories with no assignments
            var queryACategory = from ac in db.AssignmentCategories
                                 where ac.Assignments.Count() > 0 && ac.ClassId == ClassID
                                 select ac;


            double totalPercent;
            double pointsScored;
            double totalMaxPoints;
            double scaleFactor = 0.0;
            double finalGrade = 0.0;
            double categoriesWeight;
            string letterGrade = "";

            // for each assignment category
            foreach (var st in queryUid.ToList())
            {
                //reset for each student
                totalPercent = 0.0;
                categoriesWeight = 0.0;

                foreach (var ac in queryACategory.ToList())
                {
                    //reset for each category
                    totalMaxPoints = 0.0;
                    pointsScored = 0.0;

                    // get every assignment in the category in this class
                    var queryAssign = from a in db.Assignments
                                      where a.CategoriesNavigation.ClassId == ClassID &&
                                      ac.AcId == a.CategoriesNavigation.AcId
                                      select new 
                                      {
                                          aid = a.AId,
                                          maxPts = a.MaxPointVal
                                      };
       
                    foreach (var assign in queryAssign.ToList())
                    {
                        bool hasSubmission = false;
                        // then get every submission and compare the submission aID
                        // to the assignment aID for scoring
                        foreach (var s in querySubmissions.ToList())
                        {
                              
                            if(assign.aid == s.aid && st.Equals(s.uid)) { 
                                // add up the pointsScored and max possible for each category
                                totalMaxPoints += assign.maxPts;
                                pointsScored += (double)s.score;
                                hasSubmission = true;
                            }
                        }
                        //treat no submission as 0 score, still add the max points to total max points of this category
                        if (!hasSubmission)                       
                            totalMaxPoints += assign.maxPts;
                    }
                    // generate a total percentage by sum up all categories percentage point
                    totalPercent += (pointsScored / totalMaxPoints) * ac.GradingWeight;
                    // calculate the sum of all category grading weight
                    categoriesWeight += ac.GradingWeight;
                }
                // scale factor for the sum of category weight in case it's not 100
                scaleFactor = 100 / categoriesWeight;

                // calculate final grade
                finalGrade = totalPercent * scaleFactor;

                // convert to letter grade
                if (finalGrade <= 100 && finalGrade >= 93)
                {
                    letterGrade = "A";
                }
                else if (finalGrade < 93 && finalGrade >= 90)
                {
                    letterGrade = "A-";
                }
                else if (finalGrade < 90 && finalGrade >= 87)
                {
                    letterGrade = "B+";
                }
                else if (finalGrade < 87 && finalGrade >= 83)
                {
                    letterGrade = "B";
                }
                else if (finalGrade < 83 && finalGrade >= 80)
                {
                    letterGrade = "B-";
                }
                else if (finalGrade < 80 && finalGrade >= 77)
                {
                    letterGrade = "C+";
                }
                else if (finalGrade < 77 && finalGrade >= 73)
                {
                    letterGrade = "C";
                }
                else if (finalGrade < 73 && finalGrade >= 70)
                {
                    letterGrade = "C-";
                }
                else if (finalGrade < 70 && finalGrade >= 67)
                {
                    letterGrade = "D+";
                }
                else if (finalGrade < 67 && finalGrade >= 63)
                {
                    letterGrade = "D";
                }
                else if (finalGrade < 63 && finalGrade >= 60)
                {
                    letterGrade = "D-";
                }
                else if (finalGrade < 60)
                {
                    letterGrade = "E";
                }

                var queryGradeUpdate = from e in db.Enrollments
                                       where e.UId == st && e.ClassId == ClassID
                                       select e;

                Enrollment enrollment = queryGradeUpdate.SingleOrDefault();

                if (enrollment != null)
                {
                    enrollment.Grade = letterGrade;
                }

            }

            try
            {
                db.SaveChanges();
            }
            catch
            {

            }

            /*******End code to modify********/
        }
    }
}

