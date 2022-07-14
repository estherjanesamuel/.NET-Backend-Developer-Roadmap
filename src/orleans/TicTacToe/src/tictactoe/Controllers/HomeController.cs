using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using tictactoe.Grains;
using tictactoe.Models;

namespace tictactoe.Controllers;

public class ViewModel
{
    public string GameId { get; set; } = null!;
}

public class HomeController : Controller
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IGrainFactory grainFactory,
        ILogger<HomeController> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public IActionResult Index(Guid? id)
    {
        var vm = new ViewModel
        {
            GameId = id.HasValue ? id.Value.ToString() : ""
        };
        return View("Views/Index.cshtml",vm);
    }

    public async Task<IActionResult> Join(Guid id)
    {
        var guid = this.GetGuid();
        var player = _grainFactory.GetGrain<IPlayerGrain>(guid);
        await player.JoinGame(id);
        return RedirectToAction("Index", id);
    }
}
