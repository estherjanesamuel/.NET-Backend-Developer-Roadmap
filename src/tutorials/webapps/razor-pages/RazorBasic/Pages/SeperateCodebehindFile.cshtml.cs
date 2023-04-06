using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPageBasic.Pages;

public class SeperateCodebehindFileModel : PageModel
{
    public string Title => "Page with separate code behind file model";
    public string Message { get; private set; }

    public void OnGet()
    {
        Message = $"Generated at {DateTime.Now.ToLongTimeString() }";
    }
}
