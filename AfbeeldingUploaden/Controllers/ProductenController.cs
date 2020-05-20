using AfbeeldingUploaden.Data;
using AfbeeldingUploaden.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AfbeeldingUploaden.Controllers
{
    public class ProductenController : Controller
    {
        private readonly AfbeeldingUploadenDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductenController(AfbeeldingUploadenDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Producten
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Producten/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Producten/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Producten/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile afbeeldingBestand)
        {

            if (ModelState.IsValid)
            {
                if (afbeeldingBestand != null && afbeeldingBestand.Length > 0)
                {
                    product.Afbeelding = await SaveImage(afbeeldingBestand);
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Producten/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Producten/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile afbeeldingBestand)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (afbeeldingBestand != null && afbeeldingBestand.Length > 0)
                {
                    product.Afbeelding = await SaveImage(afbeeldingBestand);
                }
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Producten/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Producten/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        #region Helpers

        private async Task<string> SaveImage(IFormFile afbeeldingBestand)
        {
            // vervang spaties met streepjes voor een mooiere url
            string bestandsNaam = Path.GetFileName(afbeeldingBestand.FileName)
                .Replace(' ', '-');
            int nummer = 0;

            string naamZonderEtensie = Path.GetFileNameWithoutExtension(bestandsNaam);
            string extensie = Path.GetExtension(bestandsNaam);

            string opgeslagenNaam = bestandsNaam;
            string afbeeldingPad;
            do
            {
                if (nummer > 0)
                {
                    opgeslagenNaam = $"{naamZonderEtensie}({nummer}){extensie}";
                }
                string imgPad = $"{_environment.WebRootPath}/img";
                afbeeldingPad = System.IO.Path.Combine(imgPad, opgeslagenNaam);
                nummer++;
            } while (System.IO.File.Exists(afbeeldingPad));
            try
            {
                using var stream = new FileStream(afbeeldingPad, FileMode.Create);
                await afbeeldingBestand.CopyToAsync(stream);
                return opgeslagenNaam;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
