using System.Text.Json.Serialization;
using App.services;
using App.services.interfaces;
using Microsoft.AspNetCore.RateLimiting;
using services;
using services.interfaces;

var builder = WebApplication.CreateBuilder(args);


// Settings JSON Serializer
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IIngestaoService , IngestaoService>();
builder.Services.AddScoped<IChatService , ChatService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.MapSwagger();
    app.MapSwaggerUI();
//}

app.UseCors("ReactPolicy");

app.UseHttpsRedirection();

app.MapControllers();

app.UseRateLimiter();

app.Run();
