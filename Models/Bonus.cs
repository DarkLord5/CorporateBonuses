﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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
