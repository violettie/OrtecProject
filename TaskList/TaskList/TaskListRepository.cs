using TaskList.Interfaces;

namespace TaskList
{
    public class TaskListRepository : ITaskListRepository
    {
        private readonly TaskListCore _taskListCore;

        public TaskListRepository(TaskListCore taskListCore)
        {
            _taskListCore = taskListCore;
        }

        public bool AddProject(string name)
        {
            return _taskListCore.AddProject(name);
        }

        public bool AddTask(string project, string description)
        {
            return _taskListCore.AddTask(project, description);
        }

        public bool AddDeadline(string taskId, DateTime deadline)
        {
            return _taskListCore.AddDeadline(taskId, deadline);
        }

        public Dictionary<string, Dictionary<string, List<IProjectTask>>> ViewByDeadline()
        {
            var tasksWithDeadline = _taskListCore.FindTasksWithDeadlines();
            var tasksWithoutDeadline = _taskListCore.FindTasksWithoutDeadlines();

            if (tasksWithoutDeadline.Count > 0)
            {
                tasksWithDeadline.Add("No deadline", tasksWithoutDeadline);
            }
            return tasksWithDeadline;
        }
    }
}