using AfbeeldingUploaden.Models;
using Microsoft.EntityFrameworkCore;

namespace AfbeeldingUploaden.Data
{
    public class AfbeeldingUploadenDbContext: DbContext
    {
        /// <summary>
        /// De constructor van de database context
        /// </summary>
        /// <param name="options">De optie-parameters voor deze dbcontext,
        /// bijvoorbeeld de connectionstring</param>
        public AfbeeldingUploadenDbContext(DbContextOptions<AfbeeldingUploadenDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// De DbSet Products komt overeen met de tabel Products in de database
        /// </summary>
        public DbSet<Product> Products { get; set; }
    }
}
