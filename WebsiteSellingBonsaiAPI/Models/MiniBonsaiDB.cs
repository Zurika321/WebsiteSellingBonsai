using Microsoft.EntityFrameworkCore;
//using WebsiteSellingBonsai.Models;
using WebsiteSellingBonsaiAPI.Models;
public class MiniBonsaiDBAPI : DbContext
{
    public MiniBonsaiDBAPI(DbContextOptions<MiniBonsaiDBAPI> options)
        : base(options)
    {
    }

    public DbSet<Bonsai> Bonsais { get; set; }
    public DbSet<BonsaiType> Types { get; set; }
    public DbSet<GeneralMeaning> GeneralMeaning { get; set; }
    public DbSet<Style> Styles { get; set; }
    public DbSet<WebsiteSellingBonsaiAPI.Models.AdminUser> AdminUser { get; set; } = default!;
}
