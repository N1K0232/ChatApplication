var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

var app = builder.Build();
app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseRequestLocalization();

app.UseStaticFiles();
app.UseDefaultFiles();

app.UseAuthorization();

app.MapRazorPages();

await app.RunAsync();