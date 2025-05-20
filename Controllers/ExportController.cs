using Microsoft.AspNetCore.Mvc;
using VstupenkyWeb.Models;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace VstupenkyWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly VstupenkyManager _vstupenkyManager;

        public ExportController(VstupenkyManager vstupenkyManager)
        {
            _vstupenkyManager = vstupenkyManager;
        }

        [HttpGet("ExportCsv")]
        public IActionResult ExportCsv()
        {
            // Retrieve data from the database
            List<Vstupenka> vstupenky = _vstupenkyManager.VypisVstupenky();

            // Build the CSV content
            var csv = new StringBuilder();
            csv.AppendLine("Jméno,Počet,Datum Rezervace,Email"); // CSV Header

            foreach (var vstupenka in vstupenky)
            {
                csv.AppendLine($"{vstupenka.Jmeno},{vstupenka.Pocet},{vstupenka.DatumRezervace},{vstupenka.Email}");
            }

            // Convert the CSV content to a byte array
            var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());

            // Return the CSV file as a downloadable file
            return File(csvBytes, "text/csv", "Vstupenky.csv");
        }
    }
}