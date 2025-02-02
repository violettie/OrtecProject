public interface IProject
{
    string Name { get; }
    IList<ITask> Tasks { get; }
    void AddTask(ITask task);
}