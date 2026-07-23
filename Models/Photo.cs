using System;
using System.Collections.Generic;

namespace MinimalApi.Models;

public partial class Photo
{
    public int IdPhoto { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string Url { get; set; } = null!;
}
