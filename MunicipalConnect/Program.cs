using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MunicipalConnect.Data;
using MunicipalConnect.Infrastructure;
using MunicipalConnect.Infrastructure.Indexing;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// -------- Static file content types --------
var staticFileTypes = new FileExtensionContentTypeProvider();
staticFileTypes.Mappings[".heic"] = "image/heic";
staticFileTypes.Mappings[".heif"] = "image/heif";
staticFileTypes.Mappings[".webp"] = "image/webp";
builder.Services.AddSingleton(staticFileTypes);

// -------- Upload form limits --------
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50 * 1024 * 1024;
    o.MultipartHeadersLengthLimit = 64 * 1024;
});

// -------- MVC & Services --------
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IEventService, EventService>();

// -------- JSON repo + index --------
builder.Services.AddSingleton<IIssueRepository, JsonIssueRepository>();
builder.Services.AddSingleton<IIssueIndex, IssueIndex<AvlIndex<string, MunicipalConnect.Models.IssueReport>>>();

// -------- Session --------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var webroot = env.WebRootPath ?? "wwwroot";
    Directory.CreateDirectory(Path.Combine(webroot, "uploads"));
    Directory.CreateDirectory(Path.Combine(webroot, "data"));
}

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = app.Services.GetRequiredService<FileExtensionContentTypeProvider>()
});

using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IIssueRepository>();
    var idx = scope.ServiceProvider.GetRequiredService<IIssueIndex>();
    var seed = await repo.GetAllAsync();
    idx.Build(seed);
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<IEventService>();
    SeedData.Initialize(service);
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
