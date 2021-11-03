using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorporateBonuses.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<Bonus> Bonuses { get; set; }
        public DbSet<BonRequest> BonRequests { get; set; }
        public DbSet<PersonalBonus> PersonalBonuses { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }
    }
}