using JobBank.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JobBank.Data;
using JobBank.Components.Pages.JobPostPages.ViewModels;
using JobBank.Components.Pages.Home.ViewModels;
using JobBank.Services;

var builder = WebApplication.CreateBuilder(args);

// get connection string (will pick up env var ConnectionStrings__EmploymentBankContext)
var connStr = builder.Configuration.GetConnectionString("EmploymentBankContext");
if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException(
        "Connection string 'EmploymentBankContext' not found. Set environment variable 'ConnectionStrings__EmploymentBankContext'.");
}

builder.Services.AddDbContextFactory<EmploymentBankContext>(options =>
    options.UseSqlServer(connStr)
);

builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IIndexViewModel, IndexViewModel>()
                .AddTransient<IHomeViewModel, HomeViewModel>()
                .AddScoped<FilteredStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
