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

        public async Task<bool> AddProject(string name)
        {
            return await _taskListCore.AddProject(name);
        }

        public async Task<bool> AddTask(string project, string description)
        {
            return await _taskListCore.AddTask(project, description);
        }

        public async Task<bool> AddDeadline(string taskId, DateTime deadline)
        {
            return await _taskListCore.AddDeadline(taskId, deadline);
        }

        public async Task<Dictionary<string,Dictionary<string, List<IProjectTask>>>> ViewByDeadline()
        {
            var tasksWithDeadline = await _taskListCore.FindTasksWithDeadlines();
            var tasksWithoutDeadline = await _taskListCore.FindTasksWithoutDeadlines();

            if (tasksWithoutDeadline.Count > 0)
            {
                tasksWithDeadline.Add("No deadline", tasksWithoutDeadline);
            }
            return tasksWithDeadline;
        }

        public async Task<Dictionary<string,IList<IProjectTask>>> GetTodaysTasks()
        {
            return await _taskListCore.GetTodaysTasks();
        }

        public async Task<IList<IProject>> GetProjects()
        {
            return _taskListCore.Projects;
        }

        public async Task<IProjectTask?> GetTaskById(string taskId)
        {
            return await _taskListCore.FindTaskById(taskId);
        }

        public async Task<bool> MarkTaskAsDone(bool isComplete, string taskId)
        {
            return await _taskListCore.MarkTaskAsDone(isComplete, taskId);
        }
    }
}