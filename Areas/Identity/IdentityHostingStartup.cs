using System;
using CharityMS.Areas.Identity.Data;
using CharityMS.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(CharityMS.Areas.Identity.IdentityHostingStartup))]
namespace CharityMS.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<CharityMSdbContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("CharityMSdbContextConnection")));

                services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<CharityMSdbContext>();
            });
        }
    }
}