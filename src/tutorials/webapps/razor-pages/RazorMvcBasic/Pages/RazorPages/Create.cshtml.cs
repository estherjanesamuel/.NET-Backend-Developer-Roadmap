using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorMvcBasic.Data;

namespace RazorMvcBasic.Pages;

public class CreateModel : PageModel
{
    private readonly GuestBookContext _db;

    public CreateModel(GuestBookContext db)
    {
        _db = db;
    }

    [BindProperty]
    public Entry Entry {get; set;}

    public async Task<IActionResult> OnPostAsync(Entry entry)
    {
        if (!ModelState.IsValid) return Page();
        _db.Entries.Add(entry);
        await _db.SaveChangesAsync();
        return RedirectToPage("/RazorPages/Index");
    }
}