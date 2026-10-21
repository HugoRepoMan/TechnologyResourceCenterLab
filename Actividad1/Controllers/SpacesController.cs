using Actividad1.Models;
using Microsoft.AspNetCore.Mvc;

namespace TechnologyResourceCenter.Controllers
{
    public class SpacesController : Controller
    {
        // Lista estática y de solo lectura para mantener los datos vivos entre peticiones HTTP (GET y POST)
        private static readonly List<Space> _spacesList = new()
        {
            new Space { Id = 1, Name = "Laboratorio de Computación", Location = "Edificio A, Piso 2", Capacity = 30, HourlyRate = 15.00m, IsAvailable = true },
            new Space { Id = 2, Name = "Sala Multimedia", Location = "Edificio B, Planta Baja", Capacity = 15, HourlyRate = 20.00m, IsAvailable = false },
            new Space { Id = 3, Name = "Sala de Streaming", Location = "Edificio B, Piso 1", Capacity = 4, HourlyRate = 10.00m, IsAvailable = true },
            new Space { Id = 4, Name = "Sala de Innovación", Location = "Edificio C, Piso 3", Capacity = 50, HourlyRate = 35.00m, IsAvailable = true },
            new Space { Id = 5, Name = "Laboratorio de Robótica", Location = "Edificio A, Sótano", Capacity = 20, HourlyRate = 25.00m, IsAvailable = false }
        };

        // READ: Muestra todos los espacios registrados
        public IActionResult Index()
        {
            return View(_spacesList);
        }

        // READ: Muestra la información detallada de un espacio según su ID
        public IActionResult Details(int id)
        {
            Space? space = _spacesList.FirstOrDefault(s => s.Id == id);

            if (space is null)
                return NotFound();

            return View(space);
        }

        // Vista especializada: muestra únicamente los espacios que están habilitados para reserva
        public IActionResult Available()
        {
            var availableSpaces = _spacesList.Where(s => s.IsAvailable).ToList();
            return View(availableSpaces);
        }

        // CREATE (GET): Abre el formulario para registrar un nuevo espacio físico
        public IActionResult Create()
        {
            return View(new Space { IsAvailable = true, Capacity = 20, HourlyRate = 15.00m });
        }

        // CREATE (POST): Valida los datos ingresados y agrega el espacio a la lista en memoria
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Space space)
        {
            if (!ModelState.IsValid)
            {
                return View(space);
            }

            // Generación segura de ID para evitar duplicados después de eliminar elementos
            space.Id = _spacesList.Any() ? _spacesList.Max(x => x.Id) + 1 : 1;
            _spacesList.Add(space);

            return RedirectToAction(nameof(Index));
        }

        // UPDATE (GET): Carga en el formulario los datos del espacio que se quiere modificar
        public IActionResult Edit(int id)
        {
            Space? space = _spacesList.FirstOrDefault(s => s.Id == id);
            if (space is null)
            {
                return NotFound();
            }
            return View(space);
        }

        // UPDATE (POST): Aplica los cambios al espacio si las validaciones del modelo son correctas
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Space updatedSpace)
        {
            if (id != updatedSpace.Id)
            {
                return NotFound();
            }

            Space? existing = _spacesList.FirstOrDefault(s => s.Id == id);
            if (existing is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedSpace);
            }

            existing.Name = updatedSpace.Name;
            existing.Location = updatedSpace.Location;
            existing.Capacity = updatedSpace.Capacity;
            existing.HourlyRate = updatedSpace.HourlyRate;
            existing.IsAvailable = updatedSpace.IsAvailable;

            return RedirectToAction(nameof(Index));
        }

        // DELETE (GET): Presenta la vista de confirmación con los datos del espacio a eliminar
        public IActionResult Delete(int id)
        {
            Space? space = _spacesList.FirstOrDefault(s => s.Id == id);
            if (space is null)
            {
                return NotFound();
            }
            return View(space);
        }

        // DELETE (POST): Elimina el espacio de la lista una vez confirmado por el usuario
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Space? space = _spacesList.FirstOrDefault(s => s.Id == id);
            if (space is null)
            {
                return NotFound();
            }

            _spacesList.Remove(space);
            return RedirectToAction(nameof(Index));
        }
    }
}
