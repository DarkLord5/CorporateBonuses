using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CorporateBonuses.Models
{
    public class User : IdentityUser
    {
        public static object Identity { get; internal set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string Surname { get; set; }
        public int Rang { get; set; }
    }
}
