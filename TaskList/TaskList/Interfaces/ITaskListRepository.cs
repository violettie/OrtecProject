namespace TaskList.Interfaces
{
    public interface ITaskListRepository
    {
        bool AddProject(string name);
        bool AddTask(string project, string description);
        bool AddDeadline(string taskId, DateTime deadline);
        Dictionary<string, Dictionary<string, List<IProjectTask>>> ViewByDeadline();
    }
}
