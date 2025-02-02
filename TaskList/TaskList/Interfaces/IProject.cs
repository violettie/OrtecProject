public interface IProject
{
    Guid Id { get; }
    string Name { get; }
    IList<IProjectTask> Tasks { get; }
    void AddTask(IProjectTask task);
    IList<IProjectTask> FindTasksWithoutDeadlines();
    IList<IProjectTask> FindTasksWithDeadlines();
}