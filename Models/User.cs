using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace VstupenkyWeb.Models
{
    public class User
    {
        public int Uzivatele_ID { get; set; }
        public string login { get; set; }
        public string jmeno { get; set; }
        public string prijmeni { get; set; }
        public string email { get; set; }
        public string heslo { get; set; }
        public Role prava { get; set; }
    }
}