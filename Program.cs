using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

class Program
{
    static  void Main(string[] args)
    {

        WorkloadTeachers workloadTeachers = new WorkloadTeachers()
        {
            Subject = new Subject { Id = 1, Name = "Проектирование, разработка и оптимизация веб-приложений" },
            Teachers = new List<Teacher>() { new Teacher { Id = 1, Name = "Рахманин С. В." } },
            HoursPerWeek = 3
        };
        WorkloadTeachers workloadTeachers1 = new WorkloadTeachers()
        {
            Subject = new Subject { Id = 2, Name = "Тестирование программного обеспечения\r\nкомпьютерных систем\r\n" },
            Teachers = new List<Teacher>() { new Teacher { Id = 2, Name = "Заитова Р. Р." } },
            HoursPerWeek = 4
        };
        WorkloadTeachers workloadTeachers3 = new WorkloadTeachers()
        {
            Subject = new Subject { Id = 3, Name = "МДК.02.02 Технология разработки\r\nпрограммного обеспечения" },
            Teachers = new List<Teacher>() { new Teacher { Id = 3, Name = "Андрианова Ю. С." } },
            HoursPerWeek = 6
        };
        WorkloadTeachers workloadTeachers4 = new WorkloadTeachers()
        {
            Subject = new Subject { Id = 4, Name = "МДК.02.03 Инструментальные средства\r\nразработки программного обеспечения\r\n" },
            Teachers = new List<Teacher>() { new Teacher { Id = 4, Name = "Мавлюдова Н. И." } },
            HoursPerWeek = 5
        };
        WorkloadTeachers workloadTeachers5 = new WorkloadTeachers()
        {
            Subject = new Subject { Id = 5, Name = "МДК.03.01 Внедрение и поддержка\r\nпрограммного обеспечения компьютерных\r\nсистем" },
            Teachers = new List<Teacher>() { new Teacher { Id = 3, Name = "Андрианова Ю. С." } },
            HoursPerWeek = 2
        };
        WorkloadTeachers workloadTeachers6 = new WorkloadTeachers()
        {
            Subject = new Subject { Id = 6, Name = "МДК.03.02 Организация защиты программного\r\nобеспечения компьютерных систем\r\n" },
            Teachers = new List<Teacher>() { new Teacher { Id = 6, Name = "Староверова Е. Л. " } },
            HoursPerWeek = 4
        };
        WorkloadTeachers workloadTeachers7 = new WorkloadTeachers()
        {
            Subject = new Subject { Id = 7, Name = "МДК.05.01 Основы программирования в системе\r\n1С: Предприятие 8\r\n" },
            Teachers = new List<Teacher>() { new Teacher { Id = 2, Name = "Давыдова Г. Н." } },
            HoursPerWeek = 4
        };
        WorkloadTeachers workloadTeachers8 = new WorkloadTeachers()
        {
            Subject = new Subject { Id = 8, Name = "Экономика отрасли" },
            Teachers = new List<Teacher>() { new Teacher { Id = 2, Name = "Чалова Е. А. " } },
            HoursPerWeek = 2
        };
        WorkloadTeachers workloadTeachers9 = new WorkloadTeachers()
        {
            Subject = new Subject { Id = 9, Name = "Иностранный язык в профессиональной\r\nдеятельности\r\n" },
            Teachers = new List<Teacher>() { new Teacher { Id = 2, Name = "Круглова Т. Ю. " } },
            HoursPerWeek = 2
        };
        WorkloadTeachers workloadTeachers10 = new WorkloadTeachers()
        {
            Subject = new Subject { Id = 10, Name = "Физическая культура\r\n" },
            Teachers = new List<Teacher>() { new Teacher { Id = 2, Name = "Новикова С. Ф. " } },
            HoursPerWeek = 2
        };

        //Dictionary<Class, List<WorkloadTeachers>> workload = new()
        //{

        //    { new Class() {Id=1,Name="ПБ-41" },new List<WorkloadTeachers>(){ workloadTeachers, workloadTeachers1,  workloadTeachers3, workloadTeachers4, workloadTeachers5, workloadTeachers6, workloadTeachers7, workloadTeachers8, workloadTeachers9, workloadTeachers10 } },
        //    { new Class() {Id=2,Name="ПБ-42" },new List<WorkloadTeachers>(){ workloadTeachers, workloadTeachers1,  workloadTeachers3, workloadTeachers4, workloadTeachers5, workloadTeachers6, workloadTeachers7, workloadTeachers8, workloadTeachers9, workloadTeachers10 } }

        //};
        //Scheduler scheduler = new Scheduler();
        //Schedule schedule = new Schedule();
        //schedule = scheduler.GenerateSchedule(workload);
        //PrintSchedule(schedule);

        

        try
        {

            string filePath = "C:\\Users\\User\\Desktop\\responseDepotHelperJson.json";
            string json = File.ReadAllText(filePath);
            JArray jArray = PrepareData(json, "oneSem");


            Scheduler scheduler = new Scheduler();
            Schedule schedule = new Schedule();
            Dictionary<Class, List<WorkloadTeachers>> workload = new();
            List<SchedulerAdapter> schedulerAdapters = JsonConvert.DeserializeObject<List<SchedulerAdapter>>(jArray);
            schedulerAdapters.ForEach(o => workload.Add(o.Group, o.WorkloadTeachers));
            schedule = scheduler.GenerateSchedule(workload);
            PrintSchedule(schedule);

        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Файл не найден.");
        }
        catch (JsonReaderException)
        {
            Console.WriteLine("Ошибка при чтении JSON из файла.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }




    }

    private static JArray PrepareData(string json, string semestrName)
    {

            JArray originalArray = JArray.Parse(json);

            JArray transformedArray = new JArray();

            foreach (var item in originalArray)
            {
                JObject transformedItem = new JObject();
                transformedItem["Group"] = item["group"];

                JArray workloadTeachers = new JArray();

                foreach (var preloadItem in item["preload"])
                {
                    JObject workloadTeacher = new JObject();

                    workloadTeacher["subject"] = new JObject(); // Создаем пустой объект для "subject"

                    var discipline = preloadItem["Discipline"];
                    workloadTeacher["subject"]["Id"] = discipline["Id"];
                    workloadTeacher["subject"]["Name"] = discipline["Name"];
                    workloadTeacher["subject"]["Index"] = discipline["Index"];

                    workloadTeacher["teachers"] = preloadItem["appointments"];

                    int?[] courseSummary = preloadItem["PedagogicalHours"]["CourseSummary"].ToObject<int?[]>();

                    int semester = item["group"][semestrName].ToObject<int>(); // Здесь выбирается нужный семестр

                    int? hoursPerWeek = courseSummary != null && courseSummary.Length >= semester ? courseSummary[semester] : null;

                    workloadTeacher["HoursPerWeek"] = hoursPerWeek.HasValue ? hoursPerWeek.Value / item["parserData"]["TwoSemWeeks"].ToObject<int>() : null;

                    workloadTeachers.Add(workloadTeacher);
                }

                transformedItem["WorkloadTeachers"] = workloadTeachers;

                transformedArray.Add(transformedItem);
            }

            return transformedArray;
     
    }

    //static async void PrintTest() 
    //{
    //    string filePath = "C:\\Users\\User\\Desktop\\responseDepotHelperJson.json";
    //    string jsonString = await File.ReadAllTextAsync(filePath);

    //    JArray jArray = JArray.Parse(jsonString);
    //    foreach (JObject jsonObject in jArray)
    //    {
    //        JObject preload = (JObject)jsonObject["preload"];
    //        JObject group = (JObject)preload["group"];

    //        string groupName = group["Name"].ToString();
    //        Console.WriteLine(groupName);
    //    }
    //}
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
class SchedulerAdapter 
{



    internal Class Group { get ; set; }
    internal List<WorkloadTeachers> WorkloadTeachers { get ; set ; }
}
class Scheduler
{
    private List<TeacherEmployment> teacherEmployments = new();
    private List<Classroom> classroomEmployments = new();
    Schedule schedule = new Schedule();

    public Schedule GenerateSchedule(Dictionary<Class, List<WorkloadTeachers>> workload)
    {

        // Цикл по всем классам
        foreach (var classEntry in workload)
        {
            Class currentClass = classEntry.Key;
            List<WorkloadTeachers> workloadTeachers = classEntry.Value;

            // Создаем расписание для текущего класса
            Dictionary<int /*week*/, Dictionary<int /*dayOfWeek*/, Dictionary<int /*numPara*/, Tuple<Subject, List<Teacher>, Classroom>>>> classSchedule
                = new Dictionary<int, Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom>>>>();


            // Цикл по всем учебным предметам текущего класса
            foreach (WorkloadTeachers subjectEntry in workloadTeachers)
            {
                Subject subject = subjectEntry.Subject;
                //int hoursPerWeek = subjectEntry.HoursPerWeek;

                int countPara = (int)Math.Ceiling((double)subjectEntry.HoursPerWeek / 2);
                int currentPara = 0;


                // Генерируем расписание для каждой недели (2 недели)
                for (int week = 0; week < 2; week++)
                {
                    // Сбросить занятость учителей и аудиторий
                    ResetTeacherAndClassroomAvailability();

                    // Распределение занятий по неделям
                    for (int dayOfWeek = 1; dayOfWeek <= 6; dayOfWeek++) // Пн - Сб
                    {
                        for (int numPara = 1; numPara <= 6; numPara++) // 1 - 6
                        {
                            if (classSchedule.ContainsKey(week) &&
    classSchedule[week].ContainsKey(dayOfWeek) &&
    classSchedule[week][dayOfWeek].ContainsKey(numPara))
                            {
                                continue; // Пара уже занята, пропускаем
                            }
                            // Находим доступного учителя для предмета и класса
                            TeacherEmployment employment = new()
                            {
                                Teacher = subjectEntry.Teachers.First(),
                                NunWeek = week,
                                DayOfWeek = dayOfWeek,
                                NumPara = numPara,
                            };
                            bool availableTeacher = FindAvailableTeacherForSubject(employment);

                            //// Находим доступную аудиторию
                            //bool availableClassroom = FindAvailableClassroom();

                            // Если нашли учителя и аудиторию, добавляем занятие в расписание
                            if (availableTeacher && currentPara <= countPara)
                            {
                                AddToSchedule(classSchedule, week, dayOfWeek, numPara, subject, subjectEntry.Teachers, new Classroom() { Id = 1, Number = 111 });
                                currentPara++;

                                //classSchedule.Add(currentClass,new Dictionary<week, Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom>>>>) 
                                //classSchedule[numPara] = new Tuple<Subject, List<Teacher>, Classroom>(subject, new List<Teacher>(), availableClassroom);
                                //classSchedule[numPara].Item2.Add(availableTeacher);

                                // Помечаем учителя и аудиторию как занятые
                                MarkTeacherAndClassroomAsBusy(employment/*, availableClassroom*/);
                            }
                        }
                    }
                }

            }

            // Добавляем расписание для текущего класса в общее расписание
            schedule.Timetable[currentClass] = classSchedule;
        }

        return schedule;
    }

    void AddToSchedule(
           Dictionary<int, Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>>> schedule,
           int week,
           int dayOfWeek,
           int numPara,
           Subject subject,
           List<Teacher> teachers,
           Classroom? classroom)
    {
        // Проверяем и добавляем неделю, если необходимо
        if (!schedule.ContainsKey(week))
        {
            schedule[week] = new Dictionary<int, Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>>();
        }

        // Проверяем и добавляем день недели, если необходимо
        if (!schedule[week].ContainsKey(dayOfWeek))
        {
            schedule[week][dayOfWeek] = new Dictionary<int, Tuple<Subject, List<Teacher>, Classroom?>>();
        }

        // Добавляем или обновляем запись для numPara
        schedule[week][dayOfWeek][numPara] = Tuple.Create(subject, teachers, classroom);
    }

    private bool FindAvailableTeacherForSubject(TeacherEmployment employment)
    {

        foreach (var item in teacherEmployments)
        {
            if (item == employment) return false;
        }
        return true;
    }

    //private Classroom FindAvailableClassroom()
    //{
    //    // Реализуйте логику поиска доступной аудитории
    //}

    private void ResetTeacherAndClassroomAvailability()
    {
        // Сбросить занятость учителей и аудиторий перед началом генерации расписания
        teacherEmployments.Clear();
        classroomEmployments.Clear();
    }

    private void MarkTeacherAndClassroomAsBusy(TeacherEmployment employment)
    {
        // Пометить учителя и аудиторию как занятые
        teacherEmployments.Add(employment);
    }

}

class TeacherEmployment
{
    public Teacher Teacher { get; set; }
    public int NunWeek { get; set; }
    public int DayOfWeek { get; set; }
    public int NumPara { get; set; }
}
[Serializable]
class Class
{
    public int Id { get; set; }
    public string Name { get; set; }
}

class Teacher
{
    public int Id { get; set; }
    public string Name { get; set; }
    //public List<Subject> Qualification { get; set; }

}

class Subject
{
    public int Id { get; set; }
    public string Name { get; set; }

}

class Classroom
{
    public int Id { get; set; }
    public int Number { get; set; }

}

class WorkloadTeachers 
{
    public Subject Subject { get; set; }
    public List<Teacher> Teachers { get; set; }
    public int HoursPerWeek { get; set; }

}
class Workload
{
    public Dictionary<Class, List<WorkloadTeachers>> workload { get; set; }
}

class Schedule
{
    public Dictionary<Class, Dictionary<int /*week*/, Dictionary<int /*dayOfWeek*/, Dictionary<int /*numPara*/, Tuple<Subject, List<Teacher>, Classroom?>>>>> Timetable { get; set; }

    public Schedule()
    {
        Timetable = new Dictionary<Class, Dictionary<int /*week*/, Dictionary<int /*dayOfWeek*/, Dictionary<int /*numPara*/, Tuple<Subject, List<Teacher>, Classroom?>>>>>();
    }
}
