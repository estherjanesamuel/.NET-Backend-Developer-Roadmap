using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorMvcBasic.Data;

namespace RazorMvcBasic.Pages;

public class EditModel : PageModel
{
    private readonly GuestBookContext _db;

    public EditModel(GuestBookContext db)
    {
        _db = db;
    }

    [BindProperty]
    public Entry? Entry { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Entry = await _db.Entries.FindAsync(id);
        if (Entry is null) return RedirectToPage("/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        _db.Attach(Entry).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}


