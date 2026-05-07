using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SnackFlowMES.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly IConfiguration _config;
    private readonly ILogger<SettingsController> _log;

    public SettingsController(
        IConfiguration config,
        ILogger<SettingsController> log)
    {
        _config = config;
        _log = log;
    }

    // ── GET /Settings ────────────────────────────────────────
    public IActionResult Index()
    {
        return View();
    }
}
