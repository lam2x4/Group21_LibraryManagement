using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình JWT
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];
var secretKey = builder.Configuration["Jwt:SecretKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// Khởi tạo ODataConventionModelBuilder
var modelBuilder = new ODataConventionModelBuilder();
// Thêm EntitySet cho Customer
modelBuilder.EntitySet<Author>("Authors");
modelBuilder.EntitySet<Publisher>("Publishers");
modelBuilder.EntitySet<Category>("Categories");
modelBuilder.EntitySet<Book>("Books");
modelBuilder.EntitySet<BookItem>("BookItems");
modelBuilder.EntitySet<Loan>("Loans");
modelBuilder.EntitySet<Reservation>("Reservations");
modelBuilder.EntitySet<Fine>("Fines");

// Các EntitySet cho các bảng liên kết
modelBuilder.EntitySet<BookAuthor>("BookAuthors");
modelBuilder.EntitySet<BookCategory>("BookCategories");

// Thêm dịch vụ Controllers và OData cùng lúc
builder.Services.AddControllers()
    .AddOData(options =>
        options.AddRouteComponents("odata", modelBuilder.GetEdmModel()) // "odata" là tiền tố route OData
               .Filter() // Cho phép $filter
               .Select() // Cho phép $select
               .Expand() // Cho phép $expand
               .OrderBy() // Cho phép $orderby
               .Count() // Cho phép $count
               .SetMaxTop(null)); // Không giới hạn số lượng record trả về với $top
builder.Services.AddAuthorization();
// Add services to the container.
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DB")));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// Thêm dịch vụ Identity và cấu hình các lớp của bạn
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<LibraryDbContext>()
    .AddDefaultTokenProviders();


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
