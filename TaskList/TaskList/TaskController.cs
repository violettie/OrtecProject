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
        public IActionResult AddProject([FromBody] string name)
        {
            if (_taskListRepository.AddProject(name))
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost("projects/{project}/tasks")]
        public IActionResult AddTask([FromBody] string description, [FromRoute] string project)
        {
            if (_taskListRepository.AddTask(project, description))
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPut("projects/{project}/tasks/{task_id}/deadline")]
        public IActionResult AddDeadline([FromBody] string deadline, [FromRoute] string project, [FromRoute] string task_id)
        {
            if (DateTime.TryParse(deadline, out var deadlineDate))
            {
                if (_taskListRepository.AddDeadline(task_id, deadlineDate))
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpGet("view_by_deadline")]
        public IActionResult ViewByDeadline()
        {
            var tasks = _taskListRepository.ViewByDeadline();
            return Ok(tasks);
        }
    }
}