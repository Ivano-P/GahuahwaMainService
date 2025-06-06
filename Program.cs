using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GahuahwaMainService.Data;
using GahuahwaMainService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

builder.Services.AddAuthentication(x => {
    x.DefaultAuthenticateScheme = x.DefaultChallengeScheme = x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(y => {
    y.SaveToken = false;
    y.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder
            .Configuration["AppSettings:JWTSecret"]!))
    };
});

    
    
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

app.MapPost("/api/signin", async (
    UserManager<IdentityUser> UserManager, 
    [FromBody] LoginModel loginModel) => {
        var user = await UserManager.FindByEmailAsync(loginModel.Email);
        if (user != null && await UserManager.CheckPasswordAsync(user, loginModel.Password)) {
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder
                .Configuration["AppSettings:JWTSecret"]!));
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(@"UserId", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(14),
                SigningCredentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);
            return Results.Ok(new { token });
            
        }else return Results.BadRequest(new { message = "Username or password is incorrect" });
});

app.Run();
