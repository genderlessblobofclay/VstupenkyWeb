using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VstupenkyWeb.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
namespace VstupenkyWeb.Pages.Vstupenky
{
    
    public class TabulkaVstupenkyModel : PageModel
    {
        private readonly VstupenkyManager _vstupenkyManager;
        public List<Vstupenka> Vstupenky { get; set; } = new List<Vstupenka>(); // Inicializace

        public TabulkaVstupenkyModel(VstupenkyManager vstupenkyManager)
        {
            _vstupenkyManager = vstupenkyManager;
        }

        public void OnGet()
        {
            Vstupenky = _vstupenkyManager.VypisVstupenky();
        }

        public IActionResult OnPost(int id)
        {
            if (id > 0)
            {
                _vstupenkyManager.OdstranitVstupenku(id);
            }
            return RedirectToPage("./TabulkaVstupenky");
        }
    }
}

