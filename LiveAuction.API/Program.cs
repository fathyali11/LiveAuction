using Hangfire;
using LiveAuction.API.Extensions;
using LiveAuction.API.Hubs;
using LiveAuction.Application.Extensions;
using LiveAuction.Infrastructure.Extensions;
var builder = WebApplication.CreateBuilder(args);


builder.AddApiServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHangfireDashboard("/jobs");
app.UseExceptionHandler();
app.MapStaticAssets();
app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthorization();
app.MapHub<AuctionHub>("/hubs/auction");
app.MapControllers();

app.Run();
