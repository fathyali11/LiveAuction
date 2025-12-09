using CleanArchitectureSolutionName.API.Extensions;
using CleanArchitectureSolutionName.Application.Extensions;
using CleanArchitectureSolutionName.Infrastructure.Extensions;

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
