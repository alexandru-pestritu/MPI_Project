using DbProvider.Database;
using DbProvider.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDatabaseConnectionDetails>(sp => 
    new DatabaseConnectionDetails("172.20.100.2", "MPI_Database", "sa", "Password123"));

builder.Services.AddSingleton<IDbManager>(sp =>
{
    IDatabaseConnectionDetails? connectionDetails = sp.GetService<IDatabaseConnectionDetails>();
    if (connectionDetails is null)
    {
        throw new InvalidOperationException("Database connection details not found");
    }

    return new DbManager(connectionDetails);
});

builder.Services.AddSingleton<IAuthProvider>(sp =>
{
    IDbManager? dbManager = sp.GetService<IDbManager>();
    if (dbManager is null)
    {
        throw new InvalidOperationException("Database manager not found");
    }

    return new AuthProvider(dbManager);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Index}/{id?}");

app.Run();