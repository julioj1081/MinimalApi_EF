# 📸 Fotos CRUD - ASP.NET Core Minimal API

Guía paso a paso para la creación, ingeniería inversa (Scaffold) de base de datos y configuración completa de un CRUD utilizando una **Minimal API** en ASP.NET Core.

---

## 🛠️ Guía de Configuración Paso a Paso

### Paso 1: Crear el Proyecto
Abre tu terminal y ejecuta el siguiente comando para generar un nuevo proyecto de API Web en blanco:

```bash
dotnet new webapi MiMinimalApi
```

---

### Paso 2: Instalar Paquetes NuGet
Ingresa al archivo `Program.cs` y asegúrate de instalar las siguientes dependencias a través de la consola o la terminal:

```bash
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.EntityFrameworkCore.Design
```

---

### Paso 3: Agregar Servicios y CORS
Registra las políticas de CORS junto con los servicios necesarios para la generación de documentación Swagger en tu `Program.cs`:

```csharp
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
```

---

### Paso 4: Configurar Cadena de Conexión y Scaffold de Base de Datos
Agrega tu cadena de conexión en el archivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=ALBERTO;Database=FotosCrud;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Abre la **Consola del Administrador de Paquetes (PMC)** en Visual Studio y ejecuta **uno** de los siguientes comandos según tu tipo de autenticación de SQL Server para mapear tus tablas automáticamente a código C#:

**Opción A: Autenticación de Windows (Trusted Connection)**
```powershell
PM> Scaffold-DbContext "Server=ALBERTO;Database=FotosCrud;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
```

**Opción B: Autenticación por Usuario y Contraseña**
```powershell
PM> Scaffold-DbContext "Server=ALBERTO;Database=FotosCrud;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
```

Posteriormente, agrega la inyección del contexto en el archivo `Program.cs`:

```csharp
var conntection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FotosCrudContext>(options =>
{
    options.UseSqlServer(conntection);
});
```

---

### Paso 5: Crear Carpeta de Servicios
Crea una carpeta llamada `Services` en tu proyecto, añade tu clase `PhotoService` dentro de ella y regístrala en el contenedor de dependencias en `Program.cs`:

```csharp
builder.Services.AddScoped<PhotoService>();
```

---

### Paso 6: Configurar Middlewares e Inyección de Swagger
Agrega la redirección inicial y habilita los componentes de Swagger después de compilar la aplicación (`var app = builder.Build();`):

```csharp
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));
```

---

### Paso 7: Agregar Operaciones CRUD en `PhotoService`
Estructura la clase `PhotoService.cs` dentro de la carpeta `Services` para controlar la lógica de negocio y base de datos de la siguiente manera:

```csharp
using Microsoft.EntityFrameworkCore;
using MinimalApi.Models;

public class PhotoService
{
    private readonly FotosCrudContext _context;
    
    public PhotoService(FotosCrudContext context)
    {
        this._context = context;
    }

    // 1. INSERTAR
    public async Task<bool> InsertPhoto(Photo model)
    {
        var entity = new Photo()
        {
            IdPhoto = model.IdPhoto,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Url = model.Url
        };
        await _context.Photos.AddAsync(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    // 2. OBTENER TODAS
    public async Task<List<Photo>> GetAllPhotos()
    {
        return await _context.Photos.AsNoTracking().ToListAsync();
    }

    // 3. ACTUALIZAR
    public async Task<bool> Update(Photo model)
    {
        var photo = await _context.Photos.FindAsync(model.IdPhoto);
        if (photo != null)
        {
            photo.Nombre = model.Nombre;
            photo.Descripcion = model.Descripcion;
            photo.Url = model.Url;
            
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        return false;
    }

    // 4. ELIMINAR
    public async Task<bool> Delete(int IdPhoto)
    {
        var photo = await _context.Photos.FindAsync(IdPhoto);
        if (photo != null)
        {
            _context.Photos.Remove(photo);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        return false;
    }
}
```

---

### Paso 8: Mapear Funciones (Endpoints) de la API
Termina de enlazar las peticiones HTTP con tu servicio de fotos agregando las siguientes rutas en `Program.cs`:

```csharp
app.UseCors("*");

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
    return Results.Ok(new { mensaje = "Imagen insertada" });
});

// 3. ACTUALIZAR
app.MapPut("/api/photo", async (Photo model, PhotoService photoService) =>
{
    var actualizado = await photoService.Update(model);

    if (!actualizado)
    {
        return Results.NotFound(new { mensaje = "No se encontró la foto para actualizar" });
    }

    return Results.Ok(new { mensaje = "Modifico la imagen" });
});

// 4. ELIMINAR UNA FOTO
app.MapDelete("/api/photo/{id:int}", async (int id, PhotoService photoService) =>
{
    var eliminado = await photoService.Delete(id);

    if (!eliminado)
    {
        return Results.NotFound(new { mensaje = \$"No se encontró la foto con ID {id}" });
    }

    return Results.Ok(new { mensaje = "Foto eliminada correctamente" });
});

app.Run();
```
