using Microsoft.EntityFrameworkCore;
using WebApi.Extensions;
using WebApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DB")));

// Sử dụng các phương thức mở rộng đã tạo
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddODataServices();
builder.Services.AddIdentityServices();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGenWithAuth();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();