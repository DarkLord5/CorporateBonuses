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

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        [Range(0, 365)]
        public int DaysToReset { get; set; }

        [Range(0, 10000000)]
        public int Price { get; set; }
        public bool Enabled { get; set; }
    }
}
