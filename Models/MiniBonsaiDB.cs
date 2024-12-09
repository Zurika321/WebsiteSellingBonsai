using Microsoft.EntityFrameworkCore;
//using WebsiteSellingBonsai.Models;
using WebsiteSellingMiniBonsai.Models;
public class MiniBonsaiDB : DbContext
{
    public MiniBonsaiDB(DbContextOptions<MiniBonsaiDB> options)
        : base(options)
    {
    }

    public DbSet<Bonsai> Bonsais { get; set; }
    public DbSet<BonsaiType> Types { get; set; }
    public DbSet<GeneralMeaning> GeneralMeaning { get; set; }
    public DbSet<Style> Styles { get; set; }
    public DbSet<WebsiteSellingMiniBonsai.Models.AdminUser> AdminUser { get; set; } = default!;
}
