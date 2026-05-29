using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesPoint.Application.Interfaces.Repositories;
using SalesPoint.Infrastructure.Persistence;
using SalesPoint.Infrastructure.Persistence.Repositories;

namespace SalesPoint.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseOracle(
                configuration.GetConnectionString("DefaultConnection")
            );
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        return services;
    }
}