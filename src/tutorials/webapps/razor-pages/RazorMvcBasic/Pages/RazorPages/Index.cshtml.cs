using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorMvcBasic.Data;

namespace RazorMvcBasic.Pages;

public class IndexRazorPagesModel : PageModel
{
    private readonly GuestBookContext _db;

    public IndexRazorPagesModel(GuestBookContext db)
    {
        _db = db;
    }

    public List<Entry> Entries { get; private set; }

    public async Task OnGetAsync()
    {
        Entries = await _db.Entries.AsNoTracking().ToListAsync();
    }

    public async Task<IActionResult> OnPostLikeAsync(int id)
    {
        var entry = await _db.Entries.FindAsync(id);
        entry!.Likes += 1;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }


    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var entry = await _db.Entries.FindAsync(id);
        _db.Entries.Remove(entry!);
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
