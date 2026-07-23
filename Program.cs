using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Models;
using MinimalApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("*", policy => 
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

var conntection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FotosCrudContext>(options =>
{
    options.UseSqlServer(conntection);
});

builder.Services.AddScoped<PhotoService>();

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

// 1. OBTENER
app.MapGet("/api/photos", async (PhotoService photoService) =>
{
    var photos = await photoService.GetAllPhotos();
    return Results.Ok(photos);
});

// 2. INSERTAR
app.MapPost("/api/InsertPhoto", async (Photo model, PhotoService photoService) =>
{
    var insertado = await photoService.InsertPhoto(model);

    if (!insertado)
    {
        return Results.NotFound(new { mensaje = "No se inserto" });
    }
    return Results.Ok(new {mensaje = "Imagen insertada"});
});

// 3. ACTUALIZAR
app.MapPut("/api/photo", async (Photo model, PhotoService photoService) =>
{
    var actualizado = await photoService.Update(model);

    if (!actualizado)
    {
        return Results.NotFound(new { mensaje = "No se encontró la foto para actualizar" });
    }

    return Results.Ok(new {mensaje = "Modifico la imagen"});
});

// 4. ELIMINAR UNA FOTO
app.MapDelete("/api/photo/{id:int}", async (int id, PhotoService photoService) =>
{
    var eliminado = await photoService.Delete(id);

    if (!eliminado)
    {
        return Results.NotFound(new { mensaje = $"No se encontró la foto con ID {id}" });
    }

    return Results.Ok(new { mensaje = "Foto eliminada correctamente" });
});
app.Run();
