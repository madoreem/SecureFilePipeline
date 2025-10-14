using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using SecureFilePipeline.Db;
using SecureFilePipeline.Shared;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
});

var connectionString = DbConfig.GetConnectionString();
builder.Services.AddDbContext<FileMetadataContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "API running");

app.Run("http://0.0.0.0:80");

