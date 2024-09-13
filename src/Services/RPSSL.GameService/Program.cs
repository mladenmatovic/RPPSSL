using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RPSSL.GameService.Clients;
using RPSSL.GameService.Data;
using RPSSL.GameService.GameHub;
using RPSSL.GameService.Repositories;
using RPSSL.GameService.Services;

var builder = WebApplication.CreateBuilder(args);

// add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GameDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RandomNumberServiceOptions>(builder.Configuration.GetSection("RandomNumberService"));
builder.Services.AddHttpClient<IRandomNumberClient, RandomNumberClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<RandomNumberServiceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGamePlayService, GamePlayService>();
builder.Services.AddScoped<IGameLobbyService, GameLobbyService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder.WithOrigins("http://localhost:3000") // Your React app's URL
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(secretKey)
        };

        // We have to hook the OnMessageReceived event in order to
        // allow the JWT authentication handler to read the access
        // token from the query string when a WebSocket or 
        // Server-Sent Events request comes in.

        // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
        // due to a limitation in Browser APIs. We restrict it to only calls to the
        // SignalR hub in this code.
        // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
        // for more information about security considerations when using
        // the query string to transmit the access token.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/gamehub")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

// ensure db is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<GameDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
