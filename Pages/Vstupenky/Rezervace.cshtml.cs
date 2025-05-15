using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
namespace VstupenkyWeb.Pages.Vstupenky;

public class RezervaceModel : PageModel
{
    private readonly ILogger<RezervaceModel> _logger;

    public RezervaceModel(ILogger<RezervaceModel> logger)
    {
        _logger = logger;
    }
    public void OnGet()
    {

    }
}
