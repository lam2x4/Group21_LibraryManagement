using WebClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("Api", client =>
{
   
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
// Đặt Homepage/Index làm trang mặc định
app.MapGet("/", context =>
{
	context.Response.Redirect("/homepage/index");
	return Task.CompletedTask;
});
app.MapRazorPages();

app.Run();
