using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;
using Momeants.Api.Middleware;
using Momeants.Api.Options;
using Momeants.Api.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<ClerkOptions>(builder.Configuration.GetSection("Clerk"));
builder.Services.Configure<R2Options>(builder.Configuration.GetSection("R2"));

// Database
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("NeonPostgres")));

// R2 / S3
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var r2Opts = builder.Configuration.GetSection("R2").Get<R2Options>()!;
    var creds = new BasicAWSCredentials(r2Opts.AccessKeyId, r2Opts.SecretAccessKey);
    var config = new AmazonS3Config
    {
        ServiceURL = $"https://{r2Opts.AccountId}.r2.cloudflarestorage.com",
        ForcePathStyle = true
    };
    return new AmazonS3Client(creds, config);
});

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddHttpClient<IClerkService, ClerkService>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// CORS
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(p =>
        p.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<AuthMiddleware>();
app.MapControllers();

app.Run();
