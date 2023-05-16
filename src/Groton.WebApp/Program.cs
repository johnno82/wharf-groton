using Groton.Core.Data.Repositories;
using Groton.Core.Services;
using Serilog.Events;
using Serilog;

// How to configure and use Serilog for ASP.NET Core
// https://github.com/serilog/serilog-aspnetcore
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
#if DEBUG
        .WriteTo.Console()
#endif
        );

    // Add services to the container.
    builder.Services.AddRazorPages();

    string defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;";
    builder.Services.AddScoped(services => new EmployeeRepository(defaultConnectionString));
    builder.Services.AddScoped<EmployeeService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.UseSerilogRequestLogging();

    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
