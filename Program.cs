using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using LeshaGay;
using System.Security.Claims;
using ConsoleTables;
using LeshaGay.Data;
using Schedule = LeshaGay.Data.Schedule;

class Program
{
    static void Main(string[] args)
    {
        string filePath = "E:\\source\\LeshaGay\\message.json";
        string json = File.ReadAllText(filePath);
        Console.WriteLine("Подготовка данных");
        var workloadData = PrepareData(json, "oneSem");

        Console.WriteLine("Генерация");

        int threads = 1;
        int populationSize = 50;
        int numberOfGenerations = 500;
        double mutationRate = 80;
        Random random = new Random();
        // Генерация начальной популяции
        GeneticGenerator generator = new GeneticGenerator(workloadData, populationSize, numberOfGenerations, mutationRate);
        var result = generator.RunGeneticAlgorithm(threads);

        MainContext.Instance.Lessons.AddRange(result);
        MainContext.Instance.SaveChanges();

    }
    private static Dictionary<Group, List<WorkloadTeachers>> PrepareData(string json, string semesterName)
    {
        var originalArray = JArray.Parse(json);
        var transformedArray = new Dictionary<Group, List<WorkloadTeachers>>();
        var teacherCache = new Dictionary<string, Teacher>();
        var subjectCache = new Dictionary<int, Subject>();

        // Кэшируем всех учителей и предметы из базы данных заранее
        var allTeachers = MainContext.Instance.Teachers.ToList();
        foreach (var teacher in allTeachers)
        {
            teacherCache[teacher.FirstName] = teacher;
        }

        var allSubjects = MainContext.Instance.Subjects.ToList();
        foreach (var subject in allSubjects)
        {
            subjectCache[subject.Id] = subject;
        }

        // Списки для накопления новых объектов
        var newTeachers = new List<Teacher>();
        var newSubjects = new List<Subject>();

        foreach (var item in originalArray)
        {
            var group = new Group() { GroupCode = item["group"]["name"].Value<string>() };
            var workloadTeachersList = new List<WorkloadTeachers>();
            var preloadArray = item["preload"];

            var semester = item["group"][semesterName].ToObject<int>();
            var semWeeks = item["parserData"][GetSemestrFukingName(semester)].ToObject<int>();

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
                        ShortName = subjectName,
                    };
                    subjectCache[subjectId] = subject;
                    newSubjects.Add(subject);
                }

                var courseSummary = preloadItem["PedagogicalHours"]["CourseSummary"].ToObject<int?[]>();
                double? hoursBySemestr = (courseSummary != null && courseSummary.Length >= semester) ? courseSummary[semester] : null;
                double calculatedHoursPerWeek = hoursBySemestr.HasValue ? hoursBySemestr.Value / semWeeks : 0;

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
                        teacher = new Teacher { FirstName = "", LastName = teacherName, MiddleName = "" };
                        teacherCache[teacherName] = teacher;
                        newTeachers.Add(teacher);
                    }
                    teachers.Add(teacher);
                }

                workloadTeacher.Teachers = teachers;

                workloadTeachersList.Add(workloadTeacher);
            }

            transformedArray[group] = workloadTeachersList;
        }

        // Сохраняем все новые объекты за один вызов
        if (newSubjects.Any())
        {
            MainContext.Instance.AddRange(newSubjects);
        }

        if (newTeachers.Any())
        {
            MainContext.Instance.AddRange(newTeachers);
        }

        MainContext.Instance.SaveChanges();

        return transformedArray;
    }

    private static string GetSemestrFukingName(int num)
    {
        switch (num)
        {
            case 0:
                return "OneSemWeeks";
            case 1:
                return "TwoSemWeeks";
            case 2:
                return "ThreeSemWeeks";
            case 3:
                return "FourSemWeeks";
            case 4:
                return "FiveSemWeeks";
            case 5:
                return "SixSemWeeks";
            case 6:
                return "SevenSemWeeks";
            case 7:
                return "EightSemWeeks";
            case 8:
                return "NineSemWeeks";
            case 9:
                return "TeenSemWeeks";
            default:
                return "OneSemWeeks";
        }
    }
}