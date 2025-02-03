using TaskList.Interfaces;

namespace TaskList
{
    public sealed class TaskList
    {
        private const string QUIT = "quit";
        public static readonly string startupText = "Welcome to TaskList! Type 'help' for available commands.";

        private readonly IConsole console;

        private TaskListCore taskListCore;

        public static void Main(string[] args)
        {
            new TaskList(new RealConsole()).Run();
        }

        public TaskList(IConsole console)
        {
            this.console = console;
            this.taskListCore = new TaskListCore();
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
            var commandRest = SplitCommandLine(commandLine);
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

        private async Task Show()
        {
            var projects = taskListCore.Projects;
            foreach (var project in projects)
            {
                console.WriteLine(project.Name);
                WriteTasks(project.Tasks);
                console.WriteLine();
            }
        }

        private async Task Today()
        {
            var todaysProjects = taskListCore.GetTodaysTasks().Result;
            foreach (var project in todaysProjects)
            {
                console.WriteLine(project.Key);
                WriteTasks(project.Value);
                console.WriteLine();
            }
        }

        private async Task Add(string commandLine)
        {
            var subcommandRest = SplitCommandLine(commandLine);
            var subcommand = subcommandRest[0];
            if (subcommand == "project")
            {
                AddProject(subcommandRest[1]);
            }
            else if (subcommand == "task")
            {
                var projectTask = SplitCommandLine(subcommandRest[1]);
                AddTask(projectTask[0], projectTask[1]);
            }
        }

        private async Task AddProject(string name)
        {
            var success = taskListCore.AddProject(name);
            if (!success.Result)
            {
                console.WriteLine($"A project with the name \"{name}\" already exists.");
                return;
            }
        }

        private async Task AddTask(string project, string description)
        {
            var success = taskListCore.AddTask(project, description);
            if (!success.Result)
            {
                console.WriteLine($"Could not find a project with the name \"{project}\".");
                return;
            }
        }

        private async Task Check(string idString)
        {
            SetDone(idString, true);
        }

        private async Task Uncheck(string idString)
        {
            SetDone(idString, false);
        }

        private async Task SetDone(string idString, bool done)
        {
            var success = taskListCore.MarkTaskAsDone(done, idString);
            
            if (!success.Result)
            {
                console.WriteLine($"Could not find a task with an ID of {idString}.");
                return;
            }
        }

        private async Task AddDeadline(string commandLine)
        {
            var subcommandRest = SplitCommandLine(commandLine);
            string deadline = subcommandRest[1];
            string idString = subcommandRest[0];

            if (DateTime.TryParse(deadline, out DateTime deadlineDate))
            {
                var success = taskListCore.AddDeadline(idString, deadlineDate).Result;
                if (!success)
                {
                    console.WriteLine($"Could not find a task with an ID of {idString}.");
                    return;
                }
            }
            else
            {
                console.WriteLine("Invalid deadline date");
            }
        }

        private async Task ViewByDeadline()
        {
            var tasksWithoutDeadlines = taskListCore.FindTasksWithoutDeadlines().Result;

            var tasksWithDeadlines = taskListCore.FindTasksWithDeadlines().Result;

            foreach (var project in tasksWithDeadlines)
            {
                console.WriteLine(project.Key + ":");
                WriteTasksByDeadline(project.Value);
                console.WriteLine();
            }

            if (tasksWithoutDeadlines.Count > 0)
            {
                console.WriteLine("No deadline:");
                WriteTasksByDeadline(tasksWithoutDeadlines);
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
            console.WriteLine($"I don't know what the command \"{command}\" is.");
        }

        private string[] SplitCommandLine(string commandLine)
        {
            return commandLine.Split(" ".ToCharArray(), 2);
        }

        private void WriteTasks(IList<IProjectTask> tasks)
        {
            foreach (var task in tasks)
            {
                var deadlineString = task.Deadline.HasValue ? " " + task.Deadline.ToString() : "";
                console.WriteLine($"    [{(task.Done ? 'x' : ' ')}] " +
                    $"{task.Id}: {task.Description}{deadlineString}");
            }
        }

        private void WriteTasksByDeadline(IDictionary<string, List<IProjectTask>> tasks)
        {
            foreach (var project in tasks)
            {
                console.WriteLine($"    {project.Key}:");
                foreach (var task in project.Value)
                {
                    console.WriteLine($"        {task.Id}: {task.Description}");
                }
            }
        }
    }
}