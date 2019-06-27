using System;
using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(GMS.Areas.Identity.IdentityHostingStartup))]
namespace GMS.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}