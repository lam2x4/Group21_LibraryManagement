using WebClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient("Api", client =>
{
    // Cấu hình địa chỉ cơ sở của WebAPI
    // Sử dụng địa chỉ localhost và port của project WebAPI của bạn
    client.BaseAddress = new Uri("https://localhost:7069/");
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();  

app.UseAuthorization();

app.MapRazorPages();

app.Run();
