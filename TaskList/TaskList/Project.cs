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

    public IList<ITask> FindTasksWithDeadlines()
    {
        return Tasks.Where(t => t.Deadline != null).OrderBy(task => task.Deadline).ToList();
    }

    public IList<ITask> FindTasksWithoutDeadlines()
    {
        return Tasks.Where(t => t.Deadline == null).ToList();
    }
}