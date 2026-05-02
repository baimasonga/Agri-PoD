using AgriPod.Application.Abstractions;
using AgriPod.Infrastructure.Otp;
using AgriPod.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgriPod.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("Database:Provider") ?? "SqlServer";
        var connectionString = configuration.GetConnectionString("AgriPod")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=AgriPod;Trusted_Connection=True;TrustServerCertificate=True";

        services.AddDbContext<AgriPodDbContext>(options =>
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        services.AddScoped<IAgriPodDbContext>(sp => sp.GetRequiredService<AgriPodDbContext>());
        services.Configure<TwilioVerifyOptions>(configuration.GetSection(TwilioVerifyOptions.SectionName));
        services.AddScoped<IOtpProvider, TwilioVerifyOtpProvider>();
        return services;
    }
}
