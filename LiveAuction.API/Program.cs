using Hangfire;
using LiveAuction.API.Extensions;
using LiveAuction.API.Hubs;
using LiveAuction.Application.Extensions;
using LiveAuction.Infrastructure.Extensions;
using LiveAuction.Infrastructure.Seeders;
using Microsoft.AspNetCore.StaticFiles;
var builder = WebApplication.CreateBuilder(args);


builder.AddApiServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();
await app.Services.SeedAsync();
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseBlazorFrameworkFiles();
app.UseHangfireDashboard("/jobs");
app.UseExceptionHandler();
app.MapStaticAssets();
app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthorization();
app.MapHub<AuctionHub>("/hubs/auction");
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();
