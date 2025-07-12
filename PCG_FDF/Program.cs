// ***********************************************************************************************************************
// Solo un encabezado demo
// ***********************************************************************************************************************

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PCG_FDF;
using PCG_FDF.Data.ComponentDI;
using PCG_FDF.Data.Localization;
using PCG_FDF.Data.DataAccess;
using Syncfusion.Blazor;
using MudBlazor.Services;
using System.Globalization;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using PCG_FDF.Data.ComponentDI.AuthManagement;
using PCG_FDF.Data.ComponentDI.Quotation;
using PCG_FDF.Data.ComponentDI.Booking;
using PCG_FDF.Utility;
using PCG_FDF.Data.ComponentDI.Tracking;
using PCG_FDF.Data.Entities;
using PCG_FDF.Data.ComponentDI.CimaSimlexUserManeuvers;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Njk5NTA3QDMyMzAyZTMyMmUzMFc1NmNtSythUjNmRmo1THArOHZ5VHlnRlNSZHZKQ1lQRzBVaXJ4M3BLRUk9");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSyncfusionBlazor();
builder.Services.AddMudServices();
builder.Services.AddScoped<ComplexTreeService>();
builder.Services.AddScoped<ComplexLevelService>();
builder.Services.AddScoped<ShoppingCartContainer>();
builder.Services.AddScoped<PCG_FDF_DB>();
builder.Services.AddSingleton<GlobalLocalizer>();
builder.Services.AddSingleton<DrawerBridge>();
builder.Services.AddScoped<ApplicationState>();
builder.Services.AddScoped<GlobalBreakpointService>();
builder.Services.AddScoped<EstimationWrapperContainer>();
builder.Services.AddScoped<QuotationDataCollection_LEGACY>();
builder.Services.AddScoped<QuotationDataCollection>();
builder.Services.AddScoped<BookingDataCollection>();
builder.Services.AddScoped<TrackingDataCollection>();
builder.Services.AddScoped<ReadOnlyQuotationData>();
builder.Services.AddScoped<LayoutController>();
builder.Services.AddScoped<CIMAChatService>();
builder.Services.AddScoped<StaticShoppingCart>();
builder.Services.AddScoped<DirectContractService>();
builder.Services.AddScoped<CimaSimlexUserManeuverServices>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

var baseAddress = builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(baseuri => new BaseUriDI(baseAddress));
builder.Services.AddScoped<WhiteLabelManager>();

if (builder.HostEnvironment.IsDevelopment())
{
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["LOCAL_API_URL"]!), Timeout = TimeSpan.FromMinutes(10) });
}
else if (builder.HostEnvironment.IsStaging())
{
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["STAGING_API_URL"]!), Timeout = TimeSpan.FromMinutes(10) });
}
else
{
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["PRODUCTION_API_URL"]!), Timeout = TimeSpan.FromMinutes(10) });
}


builder.Services.AddLocalization();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

var host = builder.Build();

// Get the service instance
var white_label = host.Services.GetRequiredService<WhiteLabelManager>();

var db = host.Services.GetRequiredService<PCG_FDF_DB>();

db.Initialize(host.Services.GetRequiredService<IAuthService>());

await white_label.Initialize();

CultureInfo culture;
var js = host.Services.GetRequiredService<IJSRuntime>();
var language_module = await js.InvokeAsync<IJSObjectReference>("import", "./scripts/global_language_module.js");
var favicon_module = await js.InvokeAsync<IJSObjectReference>("import", "./scripts/favicon_change.js");
var result = await language_module.InvokeAsync<string>("getCulture");
await favicon_module.InvokeVoidAsync("change_favicon", (int)white_label.Current_Page);

if (result != null)
{
    culture = new CultureInfo(result);
}
else
{
    culture = new CultureInfo("es-MX");
    LanguageUtil.Language = PCG_ENTITIES.Enums.ELanguage.SPANISH;
    await language_module.InvokeVoidAsync("setCulture", "es-MX");
}

await favicon_module.DisposeAsync();
await language_module.DisposeAsync();

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

LanguageUtil.SetCurrentLanguage();

await host.RunAsync();

//Solo un comentario para depurar
