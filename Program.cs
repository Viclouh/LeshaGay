using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using LeshaGay;
using LeshaGay.Data;
using System.Security.Claims;

class Program
{
    static void Main(string[] args)
    {



        string filePath = "C:\\Users\\iliya\\Downloads\\responseDepotHelperJson.json";
        string json = File.ReadAllText(filePath);
        var workloadData = PrepareData(json, "oneSem");

       

        var weeks = new List<int> { 1, 2 };
        var daysOfWeek = new List<int> { 1, 2, 3, 4, 5 };
        var numParas = new List<int> { 1, 2, 3, 4 };
        var classrooms = new List<Classroom>
        {
            new Classroom { Id = 1, Number = "Room1" },
            new Classroom { Id = 2, Number = "Room2" }
        };

        var ga = new GeneticAlgorithm(10, workloadData, classrooms, weeks, daysOfWeek, numParas, 0.05);

        for (int i = 0; i < 100; i++)
        {
            ga.Evolve();
            var bestFitness = FitnessEvaluator.Evaluate(ga.CurrentPopulation.Schedules[0]);
            Console.WriteLine($"Generation {ga.Generation} - Best fitness: {bestFitness}");
        }

        //try
        //{
        //    string filePath = "C:\\Users\\iliya\\Downloads\\responseDepotHelperJson.json";
        //    string json = File.ReadAllText(filePath);

        //    Scheduler scheduler = new Scheduler();
        //    Dictionary<Group, List<WorkloadTeachers>> workload = PrepareData(json, "oneSem");
        //    Schedule schedule = scheduler.GenerateSchedule(workload);
        //    PrintSchedule(schedule);
        //}
        //catch (FileNotFoundException)
        //{
        //    Console.WriteLine("Файл не найден.");
        //}
        //catch (JsonReaderException)
        //{
        //    Console.WriteLine("Ошибка при чтении JSON из файла.");
        //}
    }
    private static Dictionary<Group, List<WorkloadTeachers>> PrepareData(string json, string semesterName)
    {
        var originalArray = JArray.Parse(json);
        var transformedArray = new Dictionary<Group, List<WorkloadTeachers>>();
        var teacherCache = new Dictionary<string, Teacher>();

        // Кэшируем всех учителей из базы данных заранее
        var allTeachers = MainContext.Instance.Teachers.ToList();
        foreach (var teacher in allTeachers)
        {
            teacherCache[teacher.Name] = teacher;
        }

        foreach (var item in originalArray)
        {
            var group = new Group { Name = item["group"]["name"].Value<string>() };
            var workloadTeachersList = new List<WorkloadTeachers>();
            var preloadArray = item["preload"];

            var semester = item["group"][semesterName].ToObject<int>();
            var twoSemWeeks = item["parserData"]["TwoSemWeeks"].ToObject<int>();

            foreach (var preloadItem in preloadArray)
            {
                if (!preloadItem["isShowPerWeek"].Value<bool>())
                    continue;

                var discipline = preloadItem["Discipline"];
                var subject = new Subject
                {
                    Id = discipline["Id"].Value<int>(),
                    Name = discipline["Name"].Value<string>()
                };

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
                        teacher = new Teacher { Name = teacherName };
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
    static void PrintScheduleTable(Schedule schedule)
    {
        foreach (var classEntry in schedule.Timetable)
        {
            Console.WriteLine($"Class: {classEntry.Key.Name}");
            Console.WriteLine("Week  | Day | Para | Subject          | Teachers       | Classroom");

            foreach (var weekEntry in classEntry.Value)
            {
                foreach (var dayEntry in weekEntry.Value)
                {
                    foreach (var paraEntry in dayEntry.Value)
                    {
                        var (subject, teachers, classroom) = paraEntry.Value;
                        string teachersNames = string.Join(", ", teachers.Select(t => t.Name));
                        string classroomNumber = classroom != null ? classroom.Number.ToString() : "N/A";
                        Console.WriteLine($"{weekEntry.Key + 1,-6}| {dayEntry.Key,-4}| {paraEntry.Key,-5}| {subject.Name,-16}| {teachersNames,-14}| {classroomNumber,-9}");
                    }
                }
            }

            Console.WriteLine();
        }
    }
    static void PrintSchedule(Schedule schedule)
    {
        foreach (var classEntry in schedule.Timetable)
        {
            Console.WriteLine($"Class: {classEntry.Key.Name}");
            foreach (var weekEntry in classEntry.Value)
            {
                Console.WriteLine($"  Week: {weekEntry.Key + 1}");
                foreach (var dayEntry in weekEntry.Value)
                {
                    Console.WriteLine($"    Day: {dayEntry.Key}");
                    foreach (var paraEntry in dayEntry.Value)
                    {
                        var (subject, teachers, classroom) = paraEntry.Value;
                        string teachersNames = string.Join(", ", teachers.Select(t => t.Name));
                        string classroomNumber = classroom != null ? classroom.Number.ToString() : "N/A";
                        Console.WriteLine($"      Para: {paraEntry.Key}, Subject: {subject.Name}, Teachers: {teachersNames}");
                    }
                }
            }
            Console.WriteLine();
        }
    }
}
