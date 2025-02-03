using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TaskList.Interfaces;

namespace TaskList
{
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskListRepository _taskListRepository;
        private readonly IMemoryCache _cache;
        private const string cacheKey = "TaskListRepository";

        public TasksController(IMemoryCache cache)
        {
            _cache = cache;
            if (_cache.TryGetValue(cacheKey, out TaskListRepository taskListRepository))
            {
                _taskListRepository = taskListRepository;
            }
            else
            {
                _taskListRepository = new TaskListRepository(new TaskListCore());
                _cache.Set(cacheKey, _taskListRepository);
            }
        }

        [HttpPost("projects")]
        public async Task<IActionResult> AddProject([FromBody] string name)
        {
            if (await _taskListRepository.AddProject(name))
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost("projects/{project}/tasks")]
        public async Task<IActionResult> AddTask([FromBody] string description, [FromRoute] string project)
        {
            if (await _taskListRepository.AddTask(project, description))
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPut("projects/tasks/{task_id}/deadline")]
        public async Task<IActionResult> AddDeadline([FromBody] string deadline, [FromRoute] string task_id)
        {
            if (DateTime.TryParse(deadline, out var deadlineDate))
            {
                if (await _taskListRepository.AddDeadline(task_id, deadlineDate))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpGet("view_by_deadline")]
        public async Task<IActionResult> ViewByDeadline()
        {
            var tasks = await _taskListRepository.ViewByDeadline();
            return Ok(tasks);
        }

        [HttpGet("show")]
        public async Task<IActionResult> Show()
        {
            var projects = await _taskListRepository.GetProjects();
            return Ok(projects);
        }

        [HttpGet("today")]
        public async Task<IActionResult> Today()
        {
            var projects = await _taskListRepository.GetTodaysTasks();
            return Ok(projects);
        }

        [HttpGet("projects/tasks/{task_id}")]
        public async Task<IActionResult> GetTaskById([FromRoute] string task_id)
        {
            var task = await _taskListRepository.GetTaskById(task_id);
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        [HttpPut("projects/tasks/{task_id}/done")]
        public async Task<IActionResult> MarkTaskAsDone([FromBody] bool isCompleted, [FromRoute] string task_id)
        {
            if (await _taskListRepository.MarkTaskAsDone(isCompleted, task_id))
            {
                return Ok();
            }
            return NotFound();
        }
    }
}