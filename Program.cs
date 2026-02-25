using JobBank.Components;
using JobBank.Components.Pages.Home.ViewModels;
using JobBank.Components.Pages.JobPostPages.ViewModels;
using JobBank.Components.Pages.SkillPages.ViewModels;
using JobBank.Data;
using JobBank.Management;
using JobBank.Services;
using JobBank.Services.Abstraction;
using JobBank.StartUpServices;
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

builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

#region DI registration

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
