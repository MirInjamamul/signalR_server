using chat_server.Hubs;
using chat_server.Models;
using chat_server.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<RosterStoreDatabaseSettings>(builder.Configuration.GetSection(nameof(RosterStoreDatabaseSettings)));
builder.Services.Configure<MessageDatabaseSettings>(builder.Configuration.GetSection(nameof(MessageDatabaseSettings)));

builder.Services.AddSingleton<IRosterStoreDatabaseSettings>(sp => sp.GetRequiredService<IOptions<RosterStoreDatabaseSettings>>().Value);
builder.Services.AddSingleton<IMessageStoreDatabaseSettings>(ms => ms.GetRequiredService<IOptions<IMessageStoreDatabaseSettings>>().Value);

builder.Services.AddSingleton<IMongoClient>(s => new MongoClient(builder.Configuration.GetValue<string>("RosterStoreDatabaseSettings:ConnectionString")));
builder.Services.AddSingleton<IMongoClient>(s => new MongoClient(builder.Configuration.GetValue<string>("MessageDatabaseSettings:ConnectionString")));

builder.Services.AddScoped<IRosterService, RosterService>();
builder.Services.AddScoped<IMessageService, MessageService>();

builder.Services.AddCors();

builder.Services.AddSingleton<PresenceTracker>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(cp => cp
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chathub");

app.Run();
