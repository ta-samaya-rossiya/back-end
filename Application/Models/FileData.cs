namespace Application.Models;

public class FileData
{ 
    public string FileName { get; set; } = default!;
        
    public string ContentType { get; set; } = default!; 
        
    public Stream Content { get; set; } = default!;
}