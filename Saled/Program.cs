using Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;  // Add this
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(cfg =>
    {
        cfg.RequireHttpsMetadata = false;
        cfg.TokenValidationParameters = TokenService.GetTokenValidationParameters();
    });

builder.Services.AddAuthorization(cfg =>
    {
        cfg.AddPolicy("AllUsers", policy => policy.RequireClaim("type", "Admin", "Agent"));
        cfg.AddPolicy("Admin", policy => policy.RequireClaim("type", "Admin"));
        cfg.AddPolicy("Agent", policy => policy.RequireClaim("type", "Agent"));
        cfg.AddPolicy("ClearanceLevel1", policy => policy.RequireAssertion(context =>
            context.User.HasClaim(c => (c.Type == "ClearanceLevel" && (c.Value == "1" || c.Value == "2")) || (c.Type == "type" && c.Value == "Admin"))
        ));
        cfg.AddPolicy("ClearanceLevel2", policy => policy.RequireClaim("ClearanceLevel", "2"));
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Saled", Version = "v1" });  // Changed title
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    { new OpenApiSecurityScheme
            {
             Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer"}
            },
        new string[] {}
    }
    });
});

// Use JSON-backed service (loads/saves `Data/Saled.json`)
builder.Services.AddSingleton<ISaledsService, SaledServiceJson>();
builder.Services.AddSingleton<IUserService, UserServiceJson>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "saled v1"));
}

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();

app.Run();
