using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using LeshaGay;
using API.Models;
using System.Security.Claims;
using ConsoleTables;
using LeshaGay.Data;

class Program
{
    static Speciality GlobalSpeciality= new () { Name = "", Shortname = "" };
    static void Main(string[] args)
    {
        string filePath = "C:\\Users\\iliya\\Downloads\\responseDepotHelperJson.json";
        string json = File.ReadAllText(filePath);
        Console.WriteLine("Подготовка данных");
        var workloadData = PrepareData(json, "oneSem");

        Console.WriteLine("Генерация");
        var geneticAlgorithm = new GeneticAlgorithm(workloadData, populationSize: 50, mutationRate: 0.2);
        Schedule bestSchedule = geneticAlgorithm.Run(generations: 1000);
        

        MainContext.Instance.LessonPlan.AddRange(bestSchedule.LessonPlans);
        MainContext.Instance.SaveChanges();

        // Вывод результатов
        Console.WriteLine("Готово. Счёт ошибок: " + bestSchedule.Fitness);
        //PrintSchedule(bestSchedule.LessonPlans);


    }
    private static Dictionary<Group, List<WorkloadTeachers>> PrepareData(string json, string semesterName)
    {
        var originalArray = JArray.Parse(json);
        var transformedArray = new Dictionary<Group, List<WorkloadTeachers>>();
        var teacherCache = new Dictionary<string, Teacher>();
        var subjectCache = new Dictionary<int, Subject>();

        // Кэшируем всех учителей из базы данных заранее
        var allTeachers = MainContext.Instance.Teacher.ToList();
        foreach (var teacher in allTeachers)
        {
            teacherCache[teacher.Name] = teacher;
        }

        // Кэшируем все предметы из базы данных заранее
        var allSubjects = MainContext.Instance.Subject.ToList();
        foreach (var subject in allSubjects)
        {
            subjectCache[subject.Id] = subject;
        }

        foreach (var item in originalArray)
        {
            var group = new Group() { Name = item["group"]["name"].Value<string>(), Speciality = GlobalSpeciality };
            var workloadTeachersList = new List<WorkloadTeachers>();
            var preloadArray = item["preload"];

            var semester = item["group"][semesterName].ToObject<int>();
            var twoSemWeeks = item["parserData"]["TwoSemWeeks"].ToObject<int>();

            foreach (var preloadItem in preloadArray)
            {
                if (!preloadItem["isShowPerWeek"].Value<bool>())
                    continue;

                var discipline = preloadItem["Discipline"];
                var subjectId = discipline["Id"].Value<int>();
                var subjectName = discipline["Name"].Value<string>();

                if (!subjectCache.TryGetValue(subjectId, out var subject))
                {
                    subject = new Subject
                    {
                        Id = subjectId,
                        Name = subjectName,
                        Shortname = subjectName,
                    };
                    MainContext.Instance.Add(subject);
                    MainContext.Instance.SaveChanges();
                    subjectCache[subjectId] = subject;
                }

                var courseSummary = preloadItem["PedagogicalHours"]["CourseSummary"].ToObject<int?[]>();
                int? hoursPerWeek = (courseSummary != null && courseSummary.Length >= semester) ? courseSummary[semester] : null;
                int calculatedHoursPerWeek = hoursPerWeek.HasValue ? hoursPerWeek.Value / twoSemWeeks : 0;

                var workloadTeacher = new WorkloadTeachers
                {
                    Subject = subject,
                    HoursPerWeek = calculatedHoursPerWeek
                };

                var appointments = preloadItem["appointments"]
                    .Where(appt => !string.IsNullOrWhiteSpace((string)appt["FIO"]))
                    .Select(appt => (string)appt["FIO"])
                    .Distinct();

                var teachers = new List<Teacher>();
                foreach (var teacherName in appointments)
                {
                    if (!teacherCache.TryGetValue(teacherName, out var teacher))
                    {
                        teacher = new Teacher { Name = "", Surname = teacherName, Patronymic = "" };
                        MainContext.Instance.Add(teacher);
                        MainContext.Instance.SaveChanges();
                        teacherCache[teacherName] = teacher;
                    }
                    teachers.Add(teacher);
                }

                workloadTeacher.Teachers = teachers;

                workloadTeachersList.Add(workloadTeacher);
            }

            transformedArray[group] = workloadTeachersList;
        }

        return transformedArray;
    }
    public static void PrintSchedule(List<LessonPlan> lessonPlans)
    {
        foreach (var group in lessonPlans.GroupBy(lp => lp.Group.Name))
        {
            Console.WriteLine($"groupname: {group.Key}");
            foreach (var dayGroup in group.GroupBy(lp => lp.Weekday))
            {
                Console.WriteLine($"+-------------+");
                Console.WriteLine($"| {((DayOfWeek)dayGroup.Key).ToString().ToLower()}|");
                Console.WriteLine($"+--------------+");
                foreach (var lesson in dayGroup)
                {
                    var table = new ConsoleTable("Lesson", "Teacher");
                    foreach (var lessonTeacher in lesson.LessonTeachers)
                    {
                        table.AddRow($"пара", $"{lessonTeacher.Teacher.Surname} {lessonTeacher.Teacher.Name[0]}.{lessonTeacher.Teacher.Patronymic[0]}.");
                    }
                    table.Write(Format.Alternative);
                    Console.WriteLine("--------");
                }
                Console.WriteLine($"+---------+");
            }
        }
    }
}

// Классы модели

    //static void PrintScheduleTable(Schedule schedule)
    //{
    //    foreach (var classEntry in schedule.Timetable)
    //    {
    //        Console.WriteLine($"Class: {classEntry.Key.Name}");
    //        Console.WriteLine("Week  | Day | Para | Subject          | Teachers       | Classroom");

    //        foreach (var weekEntry in classEntry.Value)
    //        {
    //            foreach (var dayEntry in weekEntry.Value)
    //            {
    //                foreach (var paraEntry in dayEntry.Value)
    //                {
    //                    var (subject, teachers, classroom) = paraEntry.Value;
    //                    string teachersNames = string.Join(", ", teachers.Select(t => t.Name));
    //                    Console.WriteLine($"{weekEntry.Key + 1,-6}| {dayEntry.Key,-4}| {paraEntry.Key,-5}| {subject.Name,-16}| {teachersNames,-14}| {classroomNumber,-9}");
    //                }
    //            }
    //        }

    //        Console.WriteLine();
    //    }
    //}
    //static void PrintSchedule(Schedule schedule)
    //{
    //    foreach (var classEntry in schedule.Timetable)
    //    {
    //        Console.WriteLine($"Class: {classEntry.Key.Name}");
    //        foreach (var weekEntry in classEntry.Value)
    //        {
    //            Console.WriteLine($"  Week: {weekEntry.Key + 1}");
    //            foreach (var dayEntry in weekEntry.Value)
    //            {
    //                Console.WriteLine($"    Day: {dayEntry.Key}");
    //                foreach (var paraEntry in dayEntry.Value)
    //                {
    //                    var (subject, teachers, classroom) = paraEntry.Value;
    //                    string teachersNames = string.Join(", ", teachers.Select(t => t.Name));
    //                    Console.WriteLine($"      Para: {paraEntry.Key}, Subject: {subject.Name}, Teachers: {teachersNames}");
    //                }
    //            }
    //        }
    //        Console.WriteLine();
    //    }
    //}
