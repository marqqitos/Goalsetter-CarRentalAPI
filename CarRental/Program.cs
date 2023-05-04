using CarRental.DAL;
using CarRental.DTOs;
using CarRental.Services;
using CarRental.Services.Interfaces;
using CarRental.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<DbContext, RentalContext>();

builder.Services.AddScoped<IValidator<VehicleDTO>, VehicleDTOValidator>();
builder.Services.AddScoped<IValidator<ClientDTO>, ClientDTOValidator>();
builder.Services.AddScoped<IValidator<RentalDTO>, RentalDTOValidator>();

builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IRentalService, RentalService>();

builder.Services.AddDbContext<RentalContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("CarRentalDatabaseConnectionString")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<RentalContext>();
    RentalInitializer.Seed(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
