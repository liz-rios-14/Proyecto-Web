using Microsoft.EntityFrameworkCore;
using SalesPoint.Domain.Entities;

namespace SalesPoint.Infrastructure.Persistence.Seed;

public static class AppDbContextSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (!await context.Roles.AnyAsync(cancellationToken))
        {
            await context.Roles.AddRangeAsync(
                new Role("Administrator"),
                new Role("Seller"));
        }

        if (!await context.PaymentMethods.AnyAsync(cancellationToken))
        {
            await context.PaymentMethods.AddAsync(new PaymentMethod("Cash"), cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        var administratorRole = await context.Roles
            .FirstAsync(role => role.Name == "ADMINISTRATOR", cancellationToken);

        if (!await context.Users.AnyAsync(user => user.UserName == "admin", cancellationToken))
        {
            await context.Users.AddAsync(new User(
                administratorRole.Id,
                "Administrador del Sistema",
                "admin",
                "admin@salespoint.local",
                "CHANGE_ME_HASH_ADMIN_123456"), cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
