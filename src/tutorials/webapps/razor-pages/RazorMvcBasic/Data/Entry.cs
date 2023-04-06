using System.ComponentModel.DataAnnotations;

namespace RazorMvcBasic.Data;

public class Entry
{
    public int Id { get; set; }
    
    [Required, StringLength(300)]
    public string Content { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    public int Likes { get; set; }
}