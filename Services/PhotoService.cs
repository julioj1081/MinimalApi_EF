using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using MinimalApi.Models;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MinimalApi.Services
{
    public class PhotoService
    {
        private readonly FotosCrudContext _context;
        public PhotoService(FotosCrudContext context)
        {
            this._context = context;
        }

        public async Task<bool> InsertPhoto(Photo model)
        {
            var entity = new Photo()
            {
                IdPhoto = model.IdPhoto,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Url = model.Url
            };
            await _context.Photos.AddRangeAsync(entity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<Photo>> GetAllPhotos()
        {
            return await _context.Photos.AsNoTracking().ToListAsync();
        }

        public async Task<bool> Update(Photo model)
        {
            var photo = await _context.Photos.Where(x => x.IdPhoto == model.IdPhoto).FirstOrDefaultAsync();
            if(photo != null)
            {
                photo.IdPhoto = model.IdPhoto;
                photo.Nombre = model.Nombre;
                photo.Descripcion = model.Descripcion;
                photo.Url = model.Url;
                _context.Photos.Update(photo);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            return false;
        }

        public async Task<bool> Delete(int IdPhoto)
        {
            var photo = await _context.Photos.Where(x => x.IdPhoto == IdPhoto).FirstOrDefaultAsync();
            if(photo != null)
            {
                _context.Photos.Remove(photo);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            return false;
        }
    }
}
