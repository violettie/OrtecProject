namespace TaskList
{
	public class ProjectTask : IProjectTask
    {
		public long Id { get; set; }

		public string Description { get; set; }

		public bool Done { get; set; }

		public DateTime? Deadline { get; set; } = null;
	}
}