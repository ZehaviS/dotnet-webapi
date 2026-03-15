using Services;
using MyMiddleware;
using Saled.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;  // Add this
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/log.txt", fileSizeLimitBytes: 50_000_000, rollOnFileSizeLimit: true, shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
    );
});

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
        cfg.Events = new JwtBearerEvents
        {
            // SignalR sends the token as access_token in the query string when using WebSockets.
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/activityHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(cfg =>
    {
        cfg.AddPolicy("AllUsers", policy => policy.RequireAuthenticatedUser());
        cfg.AddPolicy("Admin", policy => policy.RequireAssertion(context =>
            context.User.HasClaim(c => (c.Type == "type" && c.Value == "Admin") || (c.Type == "ClearanceLevel" && c.Value == "1"))
        ));
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
// Per-user filtering: each request sees only its own records.
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ILogQueue, LogQueue>();
builder.Services.AddHostedService<LogBackgroundWorker>();
builder.Services.AddMyServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "saled v1"));
}


app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseMyLogMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR: enable hub endpoint
app.MapHub<Hubs.ActivityHub>("/activityHub");

app.Run();
