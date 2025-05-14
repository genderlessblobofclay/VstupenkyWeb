using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VstupenkyWeb.Models;
using System.Collections.Generic; // Přidána direktiva using pro List<T>
using Microsoft.AspNetCore.Authorization;
namespace VstupenkyWeb.Pages.Vstupenky
{
    
    public class PridatModel : PageModel
    {
        private readonly VstupenkyManager _vstupenkyManager;

        [BindProperty]
        public string NoveJmeno { get; set; } = ""; // Inicializace proměnné

        [BindProperty]
        public int NovyPocet { get; set; }

        public PridatModel(VstupenkyManager vstupenkyManager)
        {
            _vstupenkyManager = vstupenkyManager;
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                _vstupenkyManager.PridatVstupenku(NoveJmeno, NovyPocet);
                return RedirectToPage("./TabulkaVstupenky");
            }
            return Page();
        }
    }
}
