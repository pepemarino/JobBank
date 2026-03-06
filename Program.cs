using JobBank.Components;
using JobBank.Components.Account;
using JobBank.Components.Pages.Home.ViewModels;
using JobBank.Components.Pages.JobPostPages.ViewModels;
using JobBank.Components.Pages.SkillPages.ViewModels;
using JobBank.Data;
using JobBank.Management;
using JobBank.Models.Identity;
using JobBank.Services;
using JobBank.Services.Abstraction;
using JobBank.StartUpServices;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

#region Connection String - Fail fast is available

// get connection string (will pick up env var ConnectionStrings__EmploymentBankContext)
var connStr = builder.Configuration.GetConnectionString("EmploymentBankContext");
if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException(
        "Connection string 'EmploymentBankContext' not found. Set environment variable 'ConnectionStrings__EmploymentBankContext'.");
}

#endregion Connection String - Fail fast is available

#region Load LLM Prompts from JSON file - Fail fast is not available

string filePath = Path.Combine(AppContext.BaseDirectory, "StartUpServices/prompts.json");

if (!File.Exists(filePath))
{
    throw new FileNotFoundException($"Startup Failed: Required file '{filePath}' is missing.");
}

string jsonString = File.ReadAllText(filePath);
var llmPrompts = JsonSerializer.Deserialize<PrompService>(jsonString);

if (llmPrompts == null)
{
    throw new InvalidDataException("Startup Failed: StartUpServices/prompts.json is empty or malformed.");
}

builder.Services.AddSingleton(llmPrompts);

#endregion Load LLM Prompts from JSON file - Fail fast is not available

builder.Services.AddDbContextFactory<EmploymentBankContext>(options =>
    options.UseSqlServer(connStr)
);

builder.Services.AddDbContext<JobBankIdentityDbContext>(options =>
    options.UseSqlServer(connStr));

builder.Services.AddIdentityCore<JobBankUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<JobBankIdentityDbContext>() // This must match your Identity Context
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<JobBankUser>, IdentityNoOpEmailSender>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationBuilder();

builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

#region DI registration

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddScoped<IIndexViewModel, IndexViewModel>()
                .AddTransient<ILLMAdvisorViewModel, LLMAdvisorViewModel>()
                .AddTransient<IHomeViewModel, HomeViewModel>()
                .AddScoped<ISkillViewModel, SkillViewModel>()
                .AddScoped<ISkillMacthAnalysisViewModel, SkillMacthAnalysisViewModel>()
                .AddScoped<FilteredStateService>()
                .AddScoped<IJobPostService, JobPostService>()
                .AddScoped<ISkillsService, SkillsService>()
                .AddScoped<CareerAssistant>()
                .AddHostedService<RejectionAnalysisWorker>()
                .AddSingleton<ILLMProvider, LLMProvider>()
                .AddSingleton<JobDescriptionParser>()   // stateless, can be singleton
                .AddSingleton<RankingEngine>()          // stateless, can be singleton
                .AddSingleton<AnalysisChannel>();       // the channel needs to be a singleton

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

#endregion DI registration

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

app.UseAuthentication(); // Must be AFTER StaticFiles
app.UseAuthorization();  // Must be AFTER Authentication

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
