namespace TaskList
{
    public class TaskListCore
    {
        private long lastId = 0;
        private readonly IList<IProject> projects = new List<IProject>();

        public TaskListCore()
        {
        }

        public IList<IProject> Projects => projects;

        public async Task<bool> AddTask(string project, string description)
        {
            if (!Projects.Any(proj => proj.Name == project))
            {
                return false;
            }

            Projects.First(projects => projects.Name == project).AddTask(
                new ProjectTask
                {
                    Id = NextId(),
                    Description = description,
                    Done = false
                });

            return true;
        }

        public async Task<bool> AddProject(string name)
        {
            if (projects.Any(project => project.Name == name))
            {
                return false;
            }

            projects.Add(new Project(name));
            return true;
        }

        public async Task<Dictionary<string, IList<IProjectTask>>> GetTodaysTasks()
        {
            var todaysTasks = new Dictionary<string, IList<IProjectTask>>();
            foreach (var project in projects)
            {
                var tasks = project.Tasks.Where(task => task.Deadline.HasValue && task.Deadline.Value.Date == DateTime.Today).ToList();
                if (tasks.Count > 0)
                {
                    todaysTasks.Add(project.Name, tasks);
                }
            }
            return todaysTasks;
        }

        public async Task<Dictionary<string, Dictionary<string, List<IProjectTask>>>> FindTasksWithDeadlines()
        {
            var tasksWithDeadlines = new List<IProject>();
            foreach (var project in projects)
            {
                var tasksWithDeadline = project.FindTasksWithDeadlines();
                if (tasksWithDeadline.Count > 0)
                {
                    tasksWithDeadlines.Add(new Project(project.Name));
                    foreach (var task in tasksWithDeadline)
                    {
                        tasksWithDeadlines.First(proj => proj.Name == project.Name).AddTask(task);
                    }
                }
            }

            var tasks = tasksWithDeadlines.SelectMany(project =>
            project.Tasks.Select(task => new { ProjectName = project.Name, Task = task }))
                .GroupBy(x => x.Task.Deadline.Value.Date)
                .OrderBy(group => group.Key)
                .ToDictionary(
                    group => group.Key.ToShortDateString(),
                    group => group.GroupBy(p => p.ProjectName)
                    .ToDictionary(
                        projectGroup => projectGroup.Key,
                        projectGroup => projectGroup.Select(t => t.Task).ToList()));

            return tasks;
        }

        public async Task<Dictionary<string, List<IProjectTask>>> FindTasksWithoutDeadlines()
        {
            var tasksWithoutDeadlines = new Dictionary<string, List<IProjectTask>>();

            foreach (var project in projects)
            {
                var tasksWithoutDeadline = await Task.Run(() => project.FindTasksWithoutDeadlines());
                if (tasksWithoutDeadline.Count > 0)
                {
                    tasksWithoutDeadlines.Add(project.Name, tasksWithoutDeadline.ToList());
                }
            }

            return tasksWithoutDeadlines;
        }

        public async Task<bool> AddDeadline(string idString, DateTime deadline)
        {
            var identifiedTask = await FindTaskById(idString);
            if (identifiedTask == null)
            {
                return false;
            }

            identifiedTask.Deadline = deadline;
            return true;
        }

        public async Task<bool> MarkTaskAsDone(bool complete, string idString)
        {
            var identifiedTask = await FindTaskById(idString);
            if (identifiedTask == null)
            {
                return false;
            }

            identifiedTask.Done = complete;
            return true;
        }

        public async Task<IProjectTask?> FindTaskById(string idString)
        {
            if (int.TryParse(idString, out int id))
            {
                return await Task.Run(() => projects.Select(project => project.Tasks.FirstOrDefault(task => task.Id == id))
                    .Where(task => task != null)
                    .FirstOrDefault());
            }
            else
            {
                return null;
            }
        }

        private long NextId()
        {
            return ++lastId;
        }
    }
}