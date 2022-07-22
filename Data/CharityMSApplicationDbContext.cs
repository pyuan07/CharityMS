
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CharityMS.Data
{
    public class CharityMSApplicationDbContext: DbContext
    {
        private IConfigurationRoot _appConfig;

        public CharityMSApplicationDbContext(DbContextOptions<CharityMSApplicationDbContext> options)
            : base(options)
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                                                    .SetBasePath(Directory.GetCurrentDirectory())
                                                    .AddJsonFile("appsettings.json");
            this._appConfig = configBuilder.Build();
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConfigurationExtensions.GetConnectionString(this._appConfig, "CharityMSApplicationDbContext"));

            base.OnConfiguring(optionsBuilder);
        }



        public DbSet<CharityMS.Models.Item> Item { get; set; }
        public DbSet<CharityMS.Models.Donation> Donation { get; set; }
        public DbSet<CharityMS.Models.PickUp> PickUp { get; set; }
    }
}
