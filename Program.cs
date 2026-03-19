using JobBank.Components;
using JobBank.Components.Account;
using JobBank.Components.Pages.Home.ViewModels;
using JobBank.Components.Pages.Interviewer.ViewModels;
using JobBank.Components.Pages.JobPostPages.ViewModels;
using JobBank.Components.Pages.SkillPages.ViewModels;
using JobBank.Data;
using JobBank.Management;
using JobBank.Management.Abstraction;
using JobBank.Models.Identity;
using JobBank.Services;
using JobBank.Services.Abstraction;
using JobBank.StartUpServices;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

#region Connection String - Fail fast

var connStr = builder.Configuration.GetConnectionString("EmploymentBankContext");
if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException(
        "Connection string 'EmploymentBankContext' not found. Set environment variable 'ConnectionStrings__EmploymentBankContext'.");
}

#endregion

#region Load LLM Prompts from JSON file

string filePath = Path.Combine(AppContext.BaseDirectory, "StartUpServices/prompts.json");
if (!File.Exists(filePath))
    throw new FileNotFoundException($"Startup Failed: Required file '{filePath}' is missing.");

string jsonString = File.ReadAllText(filePath);
var llmPrompts = JsonSerializer.Deserialize<PrompService>(jsonString)
    ?? throw new InvalidDataException("Startup Failed: StartUpServices/prompts.json is empty or malformed.");

builder.Services.AddSingleton(llmPrompts);

#endregion

#region DbContexts

builder.Services.AddDbContextFactory<EmploymentBankContext>(options =>
    options.UseSqlServer(connStr));

builder.Services.AddDbContext<JobBankIdentityDbContext>(options =>
    options.UseSqlServer(connStr));

#endregion

#region Identity + Passkeys

builder.Services.AddIdentity<JobBankUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // Required for passkey (WebAuthn) support
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
})
.AddEntityFrameworkStores<JobBankIdentityDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<JobBankUser>, IdentityNoOpEmailSender>();

#endregion

#region Blazor Server Authentication State

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationBuilder();

#endregion

builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

#region DI registrations

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddScoped<IIndexViewModel, IndexViewModel>()
    .AddTransient<ILLMAdvisorViewModel, LLMAdvisorViewModel>()
    .AddTransient<IHomeViewModel, HomeViewModel>()
    .AddScoped<ISkillViewModel, SkillViewModel>()
    .AddScoped<IInterviewerViewModel, InterviewerViewModel>()
    .AddScoped<ISkillMacthAnalysisViewModel, SkillMacthAnalysisViewModel>()
    .AddScoped<FilteredStateService>()
    .AddScoped<IJobPostService, JobPostService>()
    .AddScoped<ISkillsService, SkillsService>()
    .AddScoped<CareerAssistant>()
    .AddScoped<IIdentityService, IdentityService>()
    .AddHostedService<RejectionAnalysisWorker>()
    .AddSingleton<ILLMProvider, LLMProvider>()
    .AddScoped<ILLMManager, LLMManager>()
    .AddSingleton<JobDescriptionParser>()
    .AddSingleton<RankingEngine>()
    .AddSingleton<AnalysisChannel>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

#endregion

var app = builder.Build();

#region Middleware pipeline

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

#endregion

#region Blazor endpoints

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add Identity Razor pages/endpoints
app.MapAdditionalIdentityEndpoints();

#endregion

app.Run();