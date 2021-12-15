using System.ComponentModel.DataAnnotations;

namespace CorporateBonuses.Models
{
    public class Bonus
    {
        [Key]
        public int Id { get; set;}
        [Required]
        public int Rang { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public int DaysToReset { get; set; }


        public int Price { get; set; }
        public bool Enabled { get; set; }
    }
}
