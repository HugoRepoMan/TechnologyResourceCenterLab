using Actividad1.Models;
using Microsoft.AspNetCore.Mvc;

namespace TechnologyResourceCenter.Controllers
{
    public class AccessoriesController : Controller
    {
        // Lista estática y de solo lectura para persistir los accesorios en memoria mientras corre la aplicación
        private static readonly List<Accessory> _accessoriesList = new()
        {
            new Accessory { Id = 1, Name = "Wireless Microphone", Brand = "Rode", Category = "Audio", Price = 250.00m, Quantity = 8 },
            new Accessory { Id = 2, Name = "Brio 4K Webcam", Brand = "Logitech", Category = "Video", Price = 160.00m, Quantity = 12 },
            new Accessory { Id = 3, Name = "Heavy Duty Tripod", Brand = "Manfrotto", Category = "Video", Price = 120.00m, Quantity = 5 },
            new Accessory { Id = 4, Name = "7-in-1 USB-C Hub", Brand = "Anker", Category = "Connectivity", Price = 45.00m, Quantity = 25 },
            new Accessory { Id = 5, Name = "HDMI to USB-C Adapter", Brand = "Satechi", Category = "Connectivity", Price = 20.00m, Quantity = 30 },
            new Accessory { Id = 6, Name = "8-Port Network Switch", Brand = "TP-Link", Category = "Networking", Price = 35.00m, Quantity = 15 },
            new Accessory { Id = 7, Name = "Wi-Fi 6 Router", Brand = "Asus", Category = "Networking", Price = 180.00m, Quantity = 6 },
            new Accessory { Id = 8, Name = "Noise Cancelling Headphones", Brand = "Sony", Category = "Audio", Price = 350.00m, Quantity = 10 },
            new Accessory { Id = 9, Name = "Elgato HD60 X Capture Card", Brand = "Corsair", Category = "Video", Price = 190.00m, Quantity = 4 },
            new Accessory { Id = 10, Name = "1TB Portable SSD", Brand = "SanDisk", Category = "Storage", Price = 110.00m, Quantity = 20 },
            new Accessory { Id = 11, Name = "Mechanical Keyboard MX", Brand = "Logitech", Category = "Computing", Price = 100.00m, Quantity = 14 },
            new Accessory { Id = 12, Name = "MX Master 3S Mouse", Brand = "Logitech", Category = "Computing", Price = 99.00m, Quantity = 18 },
            new Accessory { Id = 13, Name = "2TB External HDD", Brand = "Seagate", Category = "Storage", Price = 75.00m, Quantity = 22 },
            new Accessory { Id = 14, Name = "DisplayPort to HDMI Cable", Brand = "Belkin", Category = "Connectivity", Price = 15.00m, Quantity = 40 },
            new Accessory { Id = 15, Name = "Studio Monitor Speakers", Brand = "Yamaha", Category = "Audio", Price = 300.00m, Quantity = 2 }
        };

        // READ: Muestra el listado general de accesorios
        public IActionResult Index()
        {
            return View(_accessoriesList);
        }

        // READ: Muestra el detalle de un accesorio específico validando que el ID exista
        public IActionResult Details(int id)
        {
            Accessory? accessory = _accessoriesList.FirstOrDefault(a => a.Id == id);

            if (accessory is null)
                return NotFound();

            return View(accessory);
        }

        // Vista especializada: organiza los accesorios alfabéticamente por su categoría
        public IActionResult ByCategory()
        {
            var accessoriesByCategory = _accessoriesList.OrderBy(a => a.Category).ToList();
            return View(accessoriesByCategory);
        }

        // CREATE (GET): Muestra el formulario para registrar un nuevo accesorio
        public IActionResult Create()
        {
            return View(new Accessory { Quantity = 10, Price = 25.00m });
        }

        // CREATE (POST): Valida el modelo y agrega el nuevo accesorio calculando un ID libre de colisiones
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Accessory accessory)
        {
            if (!ModelState.IsValid)
            {
                return View(accessory);
            }

            accessory.Id = _accessoriesList.Any() ? _accessoriesList.Max(x => x.Id) + 1 : 1;
            _accessoriesList.Add(accessory);

            return RedirectToAction(nameof(Index));
        }

        // UPDATE (GET): Busca el accesorio por ID para cargarlo en la vista de edición
        public IActionResult Edit(int id)
        {
            Accessory? accessory = _accessoriesList.FirstOrDefault(a => a.Id == id);
            if (accessory is null)
            {
                return NotFound();
            }
            return View(accessory);
        }

        // UPDATE (POST): Guarda los cambios realizados al accesorio si el formulario es válido
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Accessory updatedAccessory)
        {
            if (id != updatedAccessory.Id)
            {
                return NotFound();
            }

            Accessory? existing = _accessoriesList.FirstOrDefault(a => a.Id == id);
            if (existing is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedAccessory);
            }

            existing.Name = updatedAccessory.Name;
            existing.Brand = updatedAccessory.Brand;
            existing.Category = updatedAccessory.Category;
            existing.Price = updatedAccessory.Price;
            existing.Quantity = updatedAccessory.Quantity;

            return RedirectToAction(nameof(Index));
        }

        // DELETE (GET): Muestra la confirmación previa a eliminar el accesorio
        public IActionResult Delete(int id)
        {
            Accessory? accessory = _accessoriesList.FirstOrDefault(a => a.Id == id);
            if (accessory is null)
            {
                return NotFound();
            }
            return View(accessory);
        }

        // DELETE (POST): Elimina el accesorio definitivamente de la lista en memoria
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Accessory? accessory = _accessoriesList.FirstOrDefault(a => a.Id == id);
            if (accessory is null)
            {
                return NotFound();
            }

            _accessoriesList.Remove(accessory);
            return RedirectToAction(nameof(Index));
        }
    }
}
