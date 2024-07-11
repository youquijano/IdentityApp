using Api;
using Api.Data;
using Api.Models;
using Api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//be abel to inject JWTService class inside our controllers
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ContextSeedService>();
// defining our IdentityCore Service
builder.Services.AddIdentityCore<User>(options =>
{
    //password confirmation
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireUppercase = false;

    //for email confirmation
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddRoles<IdentityRole>() //to be able to add roles
    .AddRoleManager<RoleManager<IdentityRole>>() //be able to make user of RoleManager
    .AddEntityFrameworkStores<Context>() //providing our context
    .AddSignInManager<SignInManager<User>>() //make use of Signin manager
    .AddUserManager<UserManager<User>>()  //make use of usernamanager to create users
    .AddDefaultTokenProviders(); //be able to create tokens for email confirmation

//be able to authenticate users using JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //validate the token based on the key we have provided inside appsettings.development.json KWT:Key
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateIssuer = true,
            ValidateAudience = false,
            //validating the expiry token
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors();

builder.Services.Configure<ApiBehaviorOptions>(options => {
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .SelectMany(x => x.Value.Errors)
            .Select(x => x.ErrorMessage).ToArray();

        var toReturn = new
        {
            Errors = errors
        };

        return new BadRequestObjectResult(toReturn);
    };
});

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminPolicy", policy => { policy.RequireRole("Admin"); });
    opt.AddPolicy("ManagerPolicy", policy => { policy.RequireRole("Manager"); });
    opt.AddPolicy("PlayerPolicy", policy => { policy.RequireRole("Player"); });

    opt.AddPolicy("AdminOrManagerPolicy", policy => { policy.RequireRole("Admin", "Manager"); }); //or
    opt.AddPolicy("AdminAndManagerPolicy", policy => { policy.RequireRole("Admin").RequireRole("Manager"); }); //and
    opt.AddPolicy("AllRolePolicy", policy => { policy.RequireRole("Admin", "Manager", "Player"); });

    opt.AddPolicy("AdminEmailPolicy", policy => policy.RequireClaim(ClaimTypes.Email, "admin@example.com"));
    opt.AddPolicy("BiSurnamePolicy", policy => policy.RequireClaim(ClaimTypes.Surname, "bi"));

    opt.AddPolicy("ManagerEmailAndWilsonSurnamePolicy", policy => policy.RequireClaim(ClaimTypes.Surname, "wilson")
        .RequireClaim(ClaimTypes.Email, "manager@example.com"));

    opt.AddPolicy("VIPPolicy", policy => policy.RequireAssertion(context => SD.VIPPolicy(context)));

});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(builder.Configuration["JWT:ClientUrl"]);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
#region Seeding
using var scope = app.Services.CreateScope();
try
{
    var contextSeedService = scope.ServiceProvider.GetService<ContextSeedService>();
    await contextSeedService.InitializeContextAsync();
}
catch (Exception e)
{
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    logger.LogError(e.Message, "Failed to initialize and seed the database");
}
#endregion
app.Run();
