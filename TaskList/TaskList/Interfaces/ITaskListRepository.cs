namespace TaskList.Interfaces
{
    public interface ITaskListRepository
    {
        Task<bool> AddProject(string name);
        Task<bool> AddTask(string project, string description);
        Task<bool> AddDeadline(string taskId, DateTime deadline);
        Task<Dictionary<string, Dictionary<string, List<IProjectTask>>>> ViewByDeadline();
        Task<Dictionary<string, IList<IProjectTask>>> GetTodaysTasks();
        Task<IList<IProject>> GetProjects();
        Task<IProjectTask?> GetTaskById(string taskId);
        Task<bool> MarkTaskAsDone(bool isComplete, string taskId);
    }
}
