public class PostTest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public  required int Id { get; set; }
}

// Intentionally made fields not required to handle error handling in POST function
public class PostTestNR
{
    public string Title { get; set; }
    public string Content { get; set; }
    public int Id { get; set; }
}