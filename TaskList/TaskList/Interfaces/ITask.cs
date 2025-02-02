public interface ITask
{
    long Id { get; set; }
    string Description { get; set; }
    bool Done { get; set; }
    DateTime? Deadline { get; set; }
}