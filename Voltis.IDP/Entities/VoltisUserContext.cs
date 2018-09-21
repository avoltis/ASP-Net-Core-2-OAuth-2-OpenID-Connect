using Microsoft.EntityFrameworkCore;

namespace Voltis.IDP.Entities
{
    public class VoltisUserContext : DbContext
    {
        public VoltisUserContext(DbContextOptions<VoltisUserContext> options)
           : base(options)
        {
           
        }

        public DbSet<User> Users { get; set; }
    }
}
