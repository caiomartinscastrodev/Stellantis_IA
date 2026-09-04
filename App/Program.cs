using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// Settings JSON Serializer
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.MapSwagger();
    app.MapSwaggerUI();
//}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
