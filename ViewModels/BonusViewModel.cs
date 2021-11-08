using CorporateBonuses.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CorporateBonuses.ViewModels
{
    public class BonusViewModel
    {
        public int Id { get; set; }
        [Required]
        [Range(1, 5)]
        public int Rang { get; set; }
        public int UserRang { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Длина свойства {0} должна быть от {2} до {1} символов")]
        public string Name { get; set; }
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Длина свойства {0} должна быть от {2} до {1} символов")]
        public string Description { get; set; }
        [Required]
        [Range(0, 365)]
        public int DaysToReset { get; set; }

        [Range(0, 10000000)]
        public int Price { get; set; }
        public bool Enabled { get; set; }
        public string EnableString { get; set; }
        public string Answer { get; set; }
    }
}
