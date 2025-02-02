public class Project : IProject
{
    public string Name { get; private set; }
    public IList<ITask> Tasks { get; private set; }

    public Project(string name)
    {
        Name = name;
        Tasks = new List<ITask>();
    }

    public void AddTask(ITask task)
    {
        Tasks.Add(task);
    }
}