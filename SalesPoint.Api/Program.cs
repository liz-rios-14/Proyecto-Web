using SalesPoint.Application.Interfaces.Services;
using SalesPoint.Application.Services;
using SalesPoint.Infrastructure;
using SalesPoint.Infrastructure.Persistence;
using SalesPoint.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddCors(options => { options.AddPolicy("ReactPolicy", policy => { policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin(); }); });

// ========================================
// NUEVO CAMBIO - APPLICATION LAYER
// Autor: Andrew
// Descripción: Registro de servicios de Application para evitar lógica en Controllers.
// ========================================
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IErrorLogService, ErrorLogService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
<<<<<<< HEAD
=======

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await AppDbContextSeeder.SeedAsync(dbContext);
}

>>>>>>> origin/feature/infrastructure-database
app.UseCors("ReactPolicy");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
