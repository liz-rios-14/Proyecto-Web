using Microsoft.EntityFrameworkCore;
using SalesPoint.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

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

        var adminUser = await context.Users
            .FirstOrDefaultAsync(user => user.UserName == "admin", cancellationToken);

        var adminPasswordHash = Hash("Admin123456");

        if (adminUser is null)
        {
            await context.Users.AddAsync(new User(
                administratorRole.Id,
                "Administrador del Sistema",
                "admin",
                "admin@salespoint.local",
                adminPasswordHash), cancellationToken);
        }
        else if (adminUser.PasswordHash == "CHANGE_ME_HASH_ADMIN_123456")
        {
            adminUser.SetPasswordHash(adminPasswordHash);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string Hash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
