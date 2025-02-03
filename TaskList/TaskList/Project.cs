public class Project : IProject
{
    public string Name { get; private set; }
    public IList<IProjectTask> Tasks { get; private set; }
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Project(string name)
    {
        Name = name;
        Tasks = new List<IProjectTask>();
    }

    public void AddTask(IProjectTask task)
    {
        Tasks.Add(task);
    }

    public IList<IProjectTask> FindTasksWithDeadlines()
    {
        return Tasks.Where(t => t.Deadline != null).OrderBy(task => task.Deadline).ToList();
    }

    public IList<IProjectTask> FindTasksWithoutDeadlines()
    {
        return Tasks.Where(t => t.Deadline == null).ToList();
    }
}