using TaskList;

namespace Tasks;

public class TaskListCoreTests
{
    private TaskListCore taskListCore;

    [SetUp]
    public void Setup()
    {
        taskListCore = new TaskListCore();
    }

    [Test]
    public async Task AddProject_ShouldAddProject_WhenProjectNameIsValid()
    {
        // Arrange
        var projectName = "New Project";

        // Act
        bool success = await taskListCore.AddProject(projectName);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(taskListCore.Projects.Any(p => p.Name == projectName));
            Assert.That(success);
        });
    }

    [Test]
    public async Task AddProject_ShouldNotAddDuplicateProject_WhenProjectNameAlreadyExists()
    {
        // Arrange
        var projectName = "Existing Project";

        // Act
        await taskListCore.AddProject(projectName);

        bool success = await taskListCore.AddProject(projectName);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(taskListCore.Projects.Any(p => p.Name == projectName));
            Assert.That(taskListCore.Projects.Count(p => p.Name == projectName), Is.LessThanOrEqualTo(1));
            Assert.That(success, Is.False);
        });
    }

    [Test]
    public async Task AddTask_ShouldAddTaskToProject_WhenProjectExists()
    {
        // Arrange
        var projectName = "Existing Project";
        var taskDescription = "New Task";
        await taskListCore.AddProject(projectName);

        // Act
        bool success = await taskListCore.AddTask(projectName, taskDescription);

        // Assert
        var project = taskListCore.Projects.First(p => p.Name == projectName);
        Assert.Multiple(() =>
        {
            Assert.That(project.Tasks.Any(t => t.Description == taskDescription));
            Assert.That(project.Tasks.Count(t => t.Description == taskDescription) == 1);
            Assert.That(success);
        });
    }

    [Test]
    public async Task AddTask_ShouldNotAddTask_WhenProjectDoesNotExist()
    {
        // Arrange
        var projectName = "Non-Existing Project";
        var taskDescription = "New Task";

        // Act
        bool success = await taskListCore.AddTask(projectName, taskDescription);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(taskListCore.Projects.Any(p => p.Name == projectName), Is.False);
            Assert.That(success, Is.False);
        });
    }

    [Test]
    public async Task MarkTaskAsDone_ShouldMarkTaskAsDone_WhenTaskExists()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;

        // Act
        bool success = await taskListCore.MarkTaskAsDone(true, taskId.ToString());

        // Assert
        var task = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First();
        Assert.Multiple(() =>
        {
            Assert.That(taskId, Is.EqualTo(task.Id));
            Assert.That(task.Done, Is.True);
            Assert.That(success);
        });
    }

    [Test]
    public async Task MarkTaskAsDone_ShouldMarkTaskAsNotDone_WhenTaskExists()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;

        // Act
        bool success = await taskListCore.MarkTaskAsDone(false, taskId.ToString());

        var task = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First();
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(taskId, Is.EqualTo(task.Id));
            Assert.That(task.Done, Is.False);
            Assert.That(success, Is.True);
        });
    }

    [Test]
    public async Task MarkTaskAsDone_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        // Arrange
        var nonExistingTaskId = "999";

        // Act
        bool success = await taskListCore.MarkTaskAsDone(true, nonExistingTaskId);

        // Assert
        Assert.That(success, Is.False);
    }

    [Test]
    public async Task AddDeadline_ShouldAddDeadlineToTask_WhenTaskExists()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        var deadline = DateTime.Today;

        // Act
        bool success = await taskListCore.AddDeadline(taskId.ToString(), deadline);
        
        // Assert
        var task = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First();
        Assert.Multiple(() =>
        {
            Assert.That(taskId, Is.EqualTo(task.Id));
            Assert.That(task.Deadline, Is.EqualTo(deadline));
            Assert.That(success);
        });
    }

    [Test]
    public async Task AddDeadline_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        // Arrange
        var nonExistingTaskId = "999";
        var deadline = DateTime.Today;
        
        // Act
        bool success = await taskListCore.AddDeadline(nonExistingTaskId, deadline);
        
        // Assert
        Assert.That(success, Is.False);
    }

    [Test]
    public async Task GetTodaysTasks_ShouldReturnTasksWithDeadlinesForToday()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today;
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        await taskListCore.AddDeadline(taskId.ToString(), deadline);

        // Act
        var todaysTasks = await taskListCore.GetTodaysTasks();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(todaysTasks, Has.Count.EqualTo(1));
            Assert.That(todaysTasks.ContainsKey(projectName));
            Assert.That(todaysTasks[projectName], Has.Count.EqualTo(1));
            Assert.That(todaysTasks[projectName].First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public async Task GetTodaysTasks_ShouldNotReturnTasksWithoutDeadlinesForToday()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today.AddDays(1);
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        await taskListCore.AddDeadline(taskId.ToString(), deadline);
        
        // Act
        var todaysTasks = await taskListCore.GetTodaysTasks();
        
        // Assert
        Assert.That(todaysTasks.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetTodaysTasks_ShouldReturnSomeTasks_WhenSomeTasksHaveDeadlinesForToday()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today;

        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        await taskListCore.AddDeadline(taskId.ToString(), deadline);

        await taskListCore.AddTask(projectName, taskDescription + " 2");
        var taskId2 = taskListCore.Projects.First(p => p.Name == projectName).Tasks.Last().Id;
        taskListCore.AddDeadline(taskId2.ToString(), DateTime.Today.AddDays(1));

        await taskListCore.AddTask(projectName, taskDescription + " 3");
        var taskId3 = taskListCore.Projects.First(p => p.Name == projectName).Tasks.Last().Id;
        await taskListCore.AddDeadline(taskId2.ToString(), DateTime.Today.AddDays(-1));

        // Act
        var todaysTasks = await taskListCore.GetTodaysTasks();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(todaysTasks, Has.Count.EqualTo(1));
            Assert.That(todaysTasks.ContainsKey(projectName));
            Assert.That(todaysTasks[projectName], Has.Count.EqualTo(1));
            Assert.That(todaysTasks[projectName].First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public async Task FindTaskById_ShouldReturnATask_WhenTaskExists()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);

        // Act
        var task = await taskListCore.FindTaskById("1");
       
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(task, Is.Not.Null);
            Assert.That(task.Id, Is.EqualTo(1));
            Assert.That(task.Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public async Task FindTaskById_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Act
        var task = await taskListCore.FindTaskById("1");

        // Assert
        Assert.That(task, Is.Null);
    }

    [Test]
    public async Task FindTaskById_ShouldReturnNull_WhenTaskIdIsNotANumber()
    {
        // Act
        var task = await taskListCore.FindTaskById("abc");
        // Assert
        Assert.That(task, Is.Null);
    }

    [Test]
    public async Task FindTasksWithDeadlines_ShouldReturnTasksWithDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today;
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        await taskListCore.AddDeadline(taskId.ToString(), deadline);
        
        // Act
        var tasksWithDeadlines = await taskListCore.FindTasksWithDeadlines();
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tasksWithDeadlines, Has.Count.EqualTo(1));
            Assert.That(tasksWithDeadlines.ContainsKey(DateTime.Today.ToShortDateString()));
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()], Has.Count.EqualTo(1));
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].Keys, Does.Contain(projectName));
            
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].
                GetValueOrDefault(projectName), Has.Count.EqualTo(1));
            
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].
                GetValueOrDefault(projectName)?.First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public async Task FindTasksWithDeadlines_ShouldNotReturnTasksWithoutDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);

        // Act
        var tasksWithDeadlines = await taskListCore.FindTasksWithDeadlines();

        // Assert
        Assert.That(tasksWithDeadlines, Is.Empty);
    }

    [Test]
    public async Task FindTasksWithDeadlines_ShouldReturnSomeTasks_WhenSomeTasksHaveDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today;
        await taskListCore.AddProject(projectName);
        
        await taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;        
        await taskListCore.AddDeadline(taskId.ToString(), deadline);
        
        await taskListCore.AddTask(projectName, taskDescription + " 2");
        
        await taskListCore.AddTask(projectName, taskDescription + " 3");
        
        // Act
        var tasksWithDeadlines = await taskListCore.FindTasksWithDeadlines();
       
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tasksWithDeadlines, Has.Count.EqualTo(1));
            Assert.That(tasksWithDeadlines.ContainsKey(DateTime.Today.ToShortDateString()));
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()], Has.Count.EqualTo(1));
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].Keys, Does.Contain(projectName));
            
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].
                GetValueOrDefault(projectName), Has.Count.EqualTo(1));
            
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].
                GetValueOrDefault(projectName)?.First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public async Task FindTasksWithoutDeadlines_ShouldReturnTasksWithoutDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);

        // Act
        var tasksWithoutDeadlines = await taskListCore.FindTasksWithoutDeadlines();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tasksWithoutDeadlines, Has.Count.EqualTo(1));
            Assert.That(tasksWithoutDeadlines.ContainsKey(projectName));
            Assert.That(tasksWithoutDeadlines[projectName], Has.Count.EqualTo(1));
            Assert.That(tasksWithoutDeadlines[projectName].First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public async Task FindTasksWithoutDeadlines_ShouldNotReturnTasksWithDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today;
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        await taskListCore.AddDeadline(taskId.ToString(), deadline);
        
        // Act
        var tasksWithoutDeadlines = await taskListCore.FindTasksWithoutDeadlines();
        
        // Assert
        Assert.That(tasksWithoutDeadlines, Is.Empty);
    }

    [Test]
    public async Task FindTasksWithoutDeadlines_ShouldReturnSomeTasks_WhenSomeTasksHaveNoDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        await taskListCore.AddProject(projectName);
        await taskListCore.AddTask(projectName, taskDescription);

        var createdProject = taskListCore.Projects.First(p => p.Name == projectName);

        await taskListCore.AddTask(projectName, taskDescription + " 2");
        var taskId2 = createdProject.Tasks.First(task => task.Description == taskDescription + " 2").Id;
        await taskListCore.AddDeadline(taskId2.ToString(), DateTime.Today.AddDays(1));

        await taskListCore.AddTask(projectName, taskDescription + " 3");
        var taskId3 = createdProject.Tasks.First(task => task.Description == taskDescription + " 3").Id;
        await taskListCore.AddDeadline(taskId3.ToString(), DateTime.Today.AddDays(-1));

        // Act
        var tasksWithoutDeadlines = await taskListCore.FindTasksWithoutDeadlines();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tasksWithoutDeadlines, Has.Count.EqualTo(1));
            Assert.That(tasksWithoutDeadlines.ContainsKey(projectName));
            Assert.That(tasksWithoutDeadlines[projectName], Has.Count.EqualTo(1));
            Assert.That(tasksWithoutDeadlines[projectName].First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public void Projects_ShouldReturnEmptyList_WhenNoProjects()
    {
        // Act
        var projects = taskListCore.Projects;

        // Assert
        Assert.That(projects, Is.Empty);
    }

    [Test]
    public async Task Projects_ShouldReturnProjects_WhenProjectsExist()
    {
        // Arrange
        var projectName = "Project";
        await taskListCore.AddProject(projectName);
        
        // Act
        var projects = taskListCore.Projects;
       
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(projects, Is.Not.Empty);
            Assert.That(projects, Has.Count.EqualTo(1));
            Assert.That(projects.First().Name, Is.EqualTo(projectName));
        });
    }
}