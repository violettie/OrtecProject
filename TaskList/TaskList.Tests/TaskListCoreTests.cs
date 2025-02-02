using TaskList;

namespace Tasks;

public class TaskListCoreTests
{
    //private Mock<IList<IProject>> projectList;
    private TaskListCore taskListCore;

    [SetUp]
    public void Setup()
    {
        //projectList = new Mock<IList<IProject>>();
        taskListCore = new TaskListCore();
    }

    [Test]
    public void AddProject_ShouldAddProject_WhenProjectNameIsValid()
    {
        // Arrange
        var projectName = "New Project";

        // Act
        bool success = taskListCore.AddProject(projectName);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(taskListCore.Projects.Any(p => p.Name == projectName));
            Assert.That(success);
        });
    }

    [Test]
    public void AddProject_ShouldNotAddDuplicateProject_WhenProjectNameAlreadyExists()
    {
        // Arrange
        var projectName = "Existing Project";

        // Act
        taskListCore.AddProject(projectName);

        bool success = taskListCore.AddProject(projectName);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(taskListCore.Projects.Any(p => p.Name == projectName));
            Assert.That(taskListCore.Projects.Count(p => p.Name == projectName), Is.LessThanOrEqualTo(1));
            Assert.That(success, Is.False);
        });
    }

    [Test]
    public void AddTask_ShouldAddTaskToProject_WhenProjectExists()
    {
        // Arrange
        var projectName = "Existing Project";
        var taskDescription = "New Task";
        taskListCore.AddProject(projectName);

        // Act
        bool success = taskListCore.AddTask(projectName, taskDescription);

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
    public void AddTask_ShouldNotAddTask_WhenProjectDoesNotExist()
    {
        // Arrange
        var projectName = "Non-Existing Project";
        var taskDescription = "New Task";

        // Act
        bool success = taskListCore.AddTask(projectName, taskDescription);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(taskListCore.Projects.Any(p => p.Name == projectName), Is.False);
            Assert.That(success, Is.False);
        });
    }

    [Test]
    public void MarkTaskAsDone_ShouldMarkTaskAsDone_WhenTaskExists()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;

        // Act
        bool success = taskListCore.MarkTaskAsDone(true, taskId.ToString());

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
    public void MarkTaskAsDone_ShouldMarkTaskAsNotDone_WhenTaskExists()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;

        // Act
        bool success = taskListCore.MarkTaskAsDone(false, taskId.ToString());

        // Assert
        var task = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First();
        Assert.That(taskId, Is.EqualTo(task.Id));
        Assert.IsFalse(task.Done);
        Assert.IsTrue(success);
    }

    [Test]
    public void MarkTaskAsDone_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        // Arrange
        var nonExistingTaskId = "999";

        // Act
        bool success = taskListCore.MarkTaskAsDone(true, nonExistingTaskId);

        // Assert
        Assert.IsFalse(success);
    }

    [Test]
    public void AddDeadline_ShouldAddDeadlineToTask_WhenTaskExists()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        var deadline = DateTime.Today;

        // Act
        bool success = taskListCore.AddDeadline(taskId.ToString(), deadline);
        
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
    public void AddDeadline_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        // Arrange
        var nonExistingTaskId = "999";
        var deadline = DateTime.Today;
        
        // Act
        bool success = taskListCore.AddDeadline(nonExistingTaskId, deadline);
        
        // Assert
        Assert.IsFalse(success);
    }

    [Test]
    public void GetTodaysTasks_ShouldReturnTasksWithDeadlinesForToday()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today;
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        taskListCore.AddDeadline(taskId.ToString(), deadline);

        // Act
        var todaysTasks = taskListCore.GetTodaysTasks();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(todaysTasks.Count, Is.EqualTo(1));
            Assert.That(todaysTasks.ContainsKey(projectName));
            Assert.That(todaysTasks[projectName].Count, Is.EqualTo(1));
            Assert.That(todaysTasks[projectName].First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public void GetTodaysTasks_ShouldNotReturnTasksWithoutDeadlinesForToday()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today.AddDays(1);
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        taskListCore.AddDeadline(taskId.ToString(), deadline);
        
        // Act
        var todaysTasks = taskListCore.GetTodaysTasks();
        
        // Assert
        Assert.That(todaysTasks.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetTodaysTasks_ShouldReturnSomeTasks_WhenSomeTasksHaveDeadlinesForToday()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today;

        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        taskListCore.AddDeadline(taskId.ToString(), deadline);

        taskListCore.AddTask(projectName, taskDescription + " 2");
        var taskId2 = taskListCore.Projects.First(p => p.Name == projectName).Tasks.Last().Id;
        taskListCore.AddDeadline(taskId2.ToString(), DateTime.Today.AddDays(1));

        taskListCore.AddTask(projectName, taskDescription + " 3");
        var taskId3 = taskListCore.Projects.First(p => p.Name == projectName).Tasks.Last().Id;
        taskListCore.AddDeadline(taskId2.ToString(), DateTime.Today.AddDays(-1));

        // Act
        var todaysTasks = taskListCore.GetTodaysTasks();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(todaysTasks.Count, Is.EqualTo(1));
            Assert.That(todaysTasks.ContainsKey(projectName));
            Assert.That(todaysTasks[projectName].Count, Is.EqualTo(1));
            Assert.That(todaysTasks[projectName].First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public void FindTaskById_ShouldReturnATask_WhenTaskExists()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);

        // Act
        var task = taskListCore.FindTaskById("1");
       
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(task, Is.Not.Null);
            Assert.That(task.Id, Is.EqualTo(1));
            Assert.That(task.Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public void FindTaskById_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Act
        var task = taskListCore.FindTaskById("1");

        // Assert
        Assert.That(task, Is.Null);
    }

    [Test]
    public void FindTaskById_ShouldReturnNull_WhenTaskIdIsNotANumber()
    {
        // Act
        var task = taskListCore.FindTaskById("abc");
        // Assert
        Assert.That(task, Is.Null);
    }

    [Test]
    public void FindTasksWithDeadlines_ShouldReturnTasksWithDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today;
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        taskListCore.AddDeadline(taskId.ToString(), deadline);
        
        // Act
        var tasksWithDeadlines = taskListCore.FindTasksWithDeadlines();
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tasksWithDeadlines.Count, Is.EqualTo(1));
            Assert.That(tasksWithDeadlines.ContainsKey(DateTime.Today.ToShortDateString()));
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].Count, Is.EqualTo(1));
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].Keys.Contains(projectName));
            
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].
                GetValueOrDefault(projectName), Has.Count.EqualTo(1));
            
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].
                GetValueOrDefault(projectName)?.First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public void FindTasksWithDeadlines_ShouldNotReturnTasksWithoutDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);

        // Act
        var tasksWithDeadlines = taskListCore.FindTasksWithDeadlines();

        // Assert
        Assert.That(tasksWithDeadlines.Count, Is.EqualTo(0));
    }

    [Test]
    public void FindTasksWithDeadlines_ShouldReturnSomeTasks_WhenSomeTasksHaveDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today;
        taskListCore.AddProject(projectName);
        
        taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;        
        taskListCore.AddDeadline(taskId.ToString(), deadline);
        
        taskListCore.AddTask(projectName, taskDescription + " 2");
        
        taskListCore.AddTask(projectName, taskDescription + " 3");
        
        // Act
        var tasksWithDeadlines = taskListCore.FindTasksWithDeadlines();
       
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tasksWithDeadlines.Count, Is.EqualTo(1));
            Assert.That(tasksWithDeadlines.ContainsKey(DateTime.Today.ToShortDateString()));
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].Count, Is.EqualTo(1));
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].Keys.Contains(projectName));
            
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].
                GetValueOrDefault(projectName), Has.Count.EqualTo(1));
            
            Assert.That(tasksWithDeadlines[DateTime.Today.ToShortDateString()].
                GetValueOrDefault(projectName)?.First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public void FindTasksWithoutDeadlines_ShouldReturnTasksWithoutDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);

        // Act
        var tasksWithoutDeadlines = taskListCore.FindTasksWithoutDeadlines();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tasksWithoutDeadlines.Count, Is.EqualTo(1));
            Assert.That(tasksWithoutDeadlines.ContainsKey(projectName));
            Assert.That(tasksWithoutDeadlines[projectName].Count, Is.EqualTo(1));
            Assert.That(tasksWithoutDeadlines[projectName].First().Description, Is.EqualTo(taskDescription));
        });
    }

    [Test]
    public void FindTasksWithoutDeadlines_ShouldNotReturnTasksWithDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        var deadline = DateTime.Today;
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);
        var taskId = taskListCore.Projects.First(p => p.Name == projectName).Tasks.First().Id;
        taskListCore.AddDeadline(taskId.ToString(), deadline);
        
        // Act
        var tasksWithoutDeadlines = taskListCore.FindTasksWithoutDeadlines();
        
        // Assert
        Assert.That(tasksWithoutDeadlines.Count, Is.EqualTo(0));
    }

    [Test]
    public void FindTasksWithoutDeadlines_ShouldReturnSomeTasks_WhenSomeTasksHaveNoDeadlines()
    {
        // Arrange
        var projectName = "Project";
        var taskDescription = "Task";
        taskListCore.AddProject(projectName);
        taskListCore.AddTask(projectName, taskDescription);

        var createdProject = taskListCore.Projects.First(p => p.Name == projectName);

        taskListCore.AddTask(projectName, taskDescription + " 2");
        var taskId2 = createdProject.Tasks.First(task => task.Description == taskDescription + " 2").Id;
        taskListCore.AddDeadline(taskId2.ToString(), DateTime.Today.AddDays(1));

        taskListCore.AddTask(projectName, taskDescription + " 3");
        var taskId3 = createdProject.Tasks.First(task => task.Description == taskDescription + " 3").Id;
        taskListCore.AddDeadline(taskId3.ToString(), DateTime.Today.AddDays(-1));

        // Act
        var tasksWithoutDeadlines = taskListCore.FindTasksWithoutDeadlines();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(tasksWithoutDeadlines.Count, Is.EqualTo(1));
            Assert.That(tasksWithoutDeadlines.ContainsKey(projectName));
            Assert.That(tasksWithoutDeadlines[projectName].Count, Is.EqualTo(1));
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
    public void Projects_ShouldReturnProjects_WhenProjectsExist()
    {
        // Arrange
        var projectName = "Project";
        taskListCore.AddProject(projectName);
        
        // Act
        var projects = taskListCore.Projects;
       
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(projects, Is.Not.Empty);
            Assert.That(projects.Count, Is.EqualTo(1));
            Assert.That(projects.First().Name, Is.EqualTo(projectName));
        });
    }
}