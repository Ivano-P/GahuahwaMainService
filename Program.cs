using GahuahwaMainService.Data;
using GahuahwaMainService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Service from IdentityCore
builder.Services
    .AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<IdentityOptions>(options => {
        options.Password.RequireDigit =  false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        
        options.User.RequireUniqueEmail = true;
    });

//injecting DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DevConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region Config. CORS
app.UseCors(options => options.WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader());
#endregion


app.UseAuthorization();

app.MapControllers();

app
    .MapGroup("/api")
    .MapIdentityApi<IdentityUser>();

app.MapPost("/api/signup", async (
    UserManager<IdentityUser> UserManager,
    [FromBody] UserRegistrationModel userRegistrationModel
) => {
    IdentityUser user = new IdentityUser() {
        UserName = userRegistrationModel.UserName,
        Email = userRegistrationModel.Email,
    };
    var result = await UserManager.CreateAsync(user, userRegistrationModel.Password);
    
    if(result.Succeeded) {
        return Results.Ok(result);
    } else {
        return Results.BadRequest(result);
    }
});

app.Run();
