using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskList
{
    public sealed class TaskList
    {
        private const string QUIT = "quit";
        public static readonly string startupText = "Welcome to TaskList! Type 'help' for available commands.";

        private readonly IDictionary<string, IList<Task>> tasks = new Dictionary<string, IList<Task>>();
        private readonly IConsole console;

        private long lastId = 0;

        public static void Main(string[] args)
        {
            new TaskList(new RealConsole()).Run();
        }

        public TaskList(IConsole console)
        {
            this.console = console;
        }

        public void Run()
        {
            console.WriteLine(startupText);
            while (true)
            {
                console.Write("> ");
                var command = console.ReadLine();
                if (command == QUIT)
                {
                    break;
                }
                Execute(command);
            }
        }

        private void Execute(string commandLine)
        {
            var commandRest = commandLine.Split(" ".ToCharArray(), 2);
            var command = commandRest[0];
            switch (command)
            {
                case "show":
                    Show();
                    break;
                case "today":
                    Today();
                    break;
                case "add":
                    Add(commandRest[1]);
                    break;
                case "check":
                    Check(commandRest[1]);
                    break;
                case "uncheck":
                    Uncheck(commandRest[1]);
                    break;
                case "deadline":
                    AddDeadline(commandRest[1]);
                    break;
                case "view-by-deadline":
                    ViewByDeadline();
                    break;
                case "help":
                    Help();
                    break;
                default:
                    Error(command);
                    break;
            }
        }

        private void Show()
        {
            foreach (var project in tasks)
            {
                console.WriteLine(project.Key);
                foreach (var task in project.Value)
                {
                    var deadlineString = task.Deadline.HasValue ? " " + task.Deadline.ToString() : "";
                    console.WriteLine($"    [{(task.Done ? 'x' : ' ')}] " +
                        $"{task.Id}: {task.Description}{deadlineString}");
                }
                console.WriteLine();
            }
        }

        private void Today()
        {
            foreach (var project in tasks)
            {
                var todaysTasks = project.Value.Where(t => t.Deadline != null
                    && t.Deadline.Value.Date == DateTime.Now.Date).ToList();

                console.WriteLine(project.Key);
                foreach (var task in todaysTasks)
                {
                    var deadlineString = task.Deadline.HasValue ? " " + task.Deadline.ToString() : "";
                    console.WriteLine($"    [{(task.Done ? 'x' : ' ')}] " +
                        $"{task.Id}: {task.Description}{deadlineString}");
                }
                console.WriteLine();
            }
        }

        private void Add(string commandLine)
        {
            var subcommandRest = commandLine.Split(" ".ToCharArray(), 2);
            var subcommand = subcommandRest[0];
            if (subcommand == "project")
            {
                AddProject(subcommandRest[1]);
            }
            else if (subcommand == "task")
            {
                var projectTask = subcommandRest[1].Split(" ".ToCharArray(), 2);
                AddTask(projectTask[0], projectTask[1]);
            }
        }

        private void AddProject(string name)
        {
            tasks[name] = new List<Task>();
        }

        private void AddTask(string project, string description)
        {
            if (!tasks.TryGetValue(project, out IList<Task> projectTasks))
            {
                Console.WriteLine("Could not find a project with the name \"{0}\".", project);
                return;
            }
            projectTasks.Add(new Task { Id = NextId(), Description = description, Done = false });
        }

        private void Check(string idString)
        {
            SetDone(idString, true);
        }

        private void Uncheck(string idString)
        {
            SetDone(idString, false);
        }

        private void SetDone(string idString, bool done)
        {
            int id = int.Parse(idString);
            var identifiedTask = tasks
                .Select(project => project.Value.FirstOrDefault(task => task.Id == id))
                .Where(task => task != null)
                .FirstOrDefault();
            if (identifiedTask == null)
            {
                console.WriteLine("Could not find a task with an ID of {0}.", id);
                return;
            }

            identifiedTask.Done = done;
        }

        private void AddDeadline(string commandLine)
        {
            var subcommandRest = commandLine.Split(" ".ToCharArray(), 2);
            string deadline = subcommandRest[1];
            string idString = subcommandRest[0];
            if (DateTime.TryParse(deadline, out DateTime deadlineDate))
            {
                int id = int.Parse(idString);
                var identifiedTask = tasks
                    .Select(project => project.Value.FirstOrDefault(task => task.Id == id))
                    .Where(task => task != null)
                    .FirstOrDefault();
                if (identifiedTask == null)
                {
                    console.WriteLine("Could not find a task with an ID of {0}.", id);
                    return;
                }
                identifiedTask.Deadline = deadlineDate;
            }
            else
            {
                console.WriteLine("Invalid deadline date");
            }
        }

        private void ViewByDeadline()
        {
            var tasksWithoutDeadlines = tasks.Where(kvp => kvp.Value.Any(task => !task.Deadline.HasValue))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Where(task => !task.Deadline.HasValue).ToList());

            var sortedTasks = tasks.SelectMany(project =>
            project.Value.Select(task => new { ProjectName = project.Key, Task = task }))
                .Where(x => x.Task.Deadline.HasValue)
                .GroupBy(x => x.Task.Deadline.Value.Date)
                .OrderBy(group => group.Key)
                .ToDictionary(
                    group => group.Key.ToShortDateString(),
                    group => group.GroupBy(p => p.ProjectName)
                    .ToDictionary(
                        projectGroup => projectGroup.Key,
                        projectGroup => projectGroup.Select(t => t.Task).ToList()));

            foreach (var project in sortedTasks)
            {
                console.WriteLine(project.Key.ToString() + ":");
                foreach (var task in project.Value)
                {
                    console.WriteLine($"    {task.Key.ToString()}:");
                    foreach (var subTask in task.Value)
                    {
                        console.WriteLine($"        {subTask.Id}: {subTask.Description}");
                    }
                    console.WriteLine();
                }
                console.WriteLine();
            }

            if (tasksWithoutDeadlines.Count > 0)
            {
                console.WriteLine("No deadline:");
                foreach (var project in tasksWithoutDeadlines)
                {
                    console.WriteLine($"    {project.Key.ToString()}:");
                    foreach (var task in project.Value)
                    {
                        console.WriteLine($"        {task.Id}: {task.Description}");
                    }
                    console.WriteLine();
                }
                console.WriteLine();
            }
        }

        private void Help()
        {
            console.WriteLine("Commands:");
            console.WriteLine("  show");
            console.WriteLine("  today");
            console.WriteLine("  add project <project name>");
            console.WriteLine("  add task <project name> <task description>");
            console.WriteLine("  check <task ID>");
            console.WriteLine("  uncheck <task ID>");
            console.WriteLine("  deadline <task ID> <date>");
            console.WriteLine("  view-by-deadline");
            console.WriteLine();
        }

        private void Error(string command)
        {
            console.WriteLine("I don't know what the command \"{0}\" is.", command);
        }

        private long NextId()
        {
            return ++lastId;
        }
    }
}
