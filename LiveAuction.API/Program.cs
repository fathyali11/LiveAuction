using LiveAuction.API.Extensions;
using LiveAuction.Application.Extensions;
using LiveAuction.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.AddApiServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
