namespace TodoApi.Models
{
    public class TodoItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
        // The DI of the optional linked contact, -1 if not set
        public int LinkedContactId { get; set; } = -1;
    }
}