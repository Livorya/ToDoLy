namespace ToDoLy.Models
{
    public class ToDoTask
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Project { get; set; }
        public bool Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Notes { get; set; }

        public string? UserName { get; set; } 
        // Made Nullable so it can be set in the controller,
        // uses the users name since its unique and reachable
        // (User.Identity.Name => returns the email)
    }
}
