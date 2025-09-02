using Noundry.Authnz.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages();

// Add Noundry OAuth services
builder.Services.AddNoundryOAuth(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add OAuth middleware
app.UseNoundryOAuth();

// Map Razor Pages
app.MapRazorPages();

app.Run();