using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VstupenkyWeb.Models;
using System.Collections.Generic; // Přidána direktiva using pro List<T>
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace VstupenkyWeb.Pages.Vstupenky
{

    public class PridatModel : PageModel
    {
        private readonly VstupenkyManager _vstupenkyManager;
        private readonly ILogger<PridatModel> _logger;

        [BindProperty]
        public string NoveJmeno { get; set; } = ""; // Inicializace proměnné

        [BindProperty]
        public int NovyPocet { get; set; }

        public PridatModel(VstupenkyManager vstupenkyManager, ILogger<PridatModel> logger)
        {
            _vstupenkyManager = vstupenkyManager;
            _logger = logger;
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                // Get the current user's ID
                var userId = _vstupenkyManager.GetCurrentUserId();

                // Get the total number of tickets reserved by the user
                var totalTickets = _vstupenkyManager.GetTotalTicketsForUser(userId);

                // Check if the user is trying to reserve more than 20 tickets
                if (totalTickets + NovyPocet > 20)
                {
                   string errorMessage = "You cannot reserve more than 20 tickets.";
                    if (totalTickets > 0)
                    {
                        errorMessage += $" You currently have {totalTickets} tickets.";
                    }

                    _logger.LogError("User {UserId} tried to reserve {NovyPocet} tickets, but the limit is 20.", userId, NovyPocet);
                    ModelState.AddModelError("", errorMessage);
                    return Page();
                }

                _vstupenkyManager.PridatVstupenku(NoveJmeno, NovyPocet);
                return RedirectToPage("./TabulkaVstupenky");
            }
            return Page();
        }
    }
}