using AfbeeldingUploaden.Data;
using AfbeeldingUploaden.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
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
            // je kan nog iets doen met het resultaat van DeleteImage (true, false)
            _ = DeleteImage(product.Afbeelding);

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
                afbeeldingPad = ImagePath(opgeslagenNaam);
                nummer++;
            } while (System.IO.File.Exists(afbeeldingPad));
            try
            {
                using var stream = new FileStream(afbeeldingPad, FileMode.Create);
                await afbeeldingBestand.CopyToAsync(stream);
            }
            catch
            {
                return string.Empty;
            }
            bool thumbnailOpgeslagen = Thumbnail(opgeslagenNaam);

            // Als geen thumbnail kan worden gemaakt, 
            // Is er waarschijnlijk iets mis met het bestand.
            // We wissen het bestand en geven een lege string terug.
            if (! thumbnailOpgeslagen)
            {
                DeleteImage(opgeslagenNaam);
                return string.Empty;
            }
            return opgeslagenNaam;
        }

        public bool ThumbnailCallback()
        {
            return false;
        }

        private bool Thumbnail(string afbeeldingNaam)
        {
            // Het is niet handig om deze waarde in de code te bepalen. 
            // Je kan het beter definieren in een settings bestand
            int thumbnailWidth = 160;

            string afbeeldingPad = ImagePath(afbeeldingNaam);
            try
            {
                Bitmap origineel = new Bitmap(afbeeldingPad);
                Image.GetThumbnailImageAbort myCallBack = new Image.GetThumbnailImageAbort(ThumbnailCallback);

                float verkleiningsFactor = (float)thumbnailWidth / (float)origineel.Width;
                int thumbnailHeight = (int)(verkleiningsFactor * origineel.Height);

                Image thumbnail = origineel
                    .GetThumbnailImage(thumbnailWidth, thumbnailHeight, myCallBack, IntPtr.Zero);

                string thumbnailNaam = $"thumb.{afbeeldingNaam}";
                string thumbnailPad = ImagePath(thumbnailNaam);

                thumbnail.Save(thumbnailPad);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool DeleteImage(string afbeeldingNaam)
        {
            try
            {
                string afbeeldingPad = ImagePath(afbeeldingNaam);
                System.IO.File.Delete(afbeeldingPad);

                string thumbnailPad = ImagePath($"thumb.{afbeeldingNaam}");
                System.IO.File.Delete(thumbnailPad);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public string ImagePath(string imageFileName)
        {
            string imgPad = $"{_environment.WebRootPath}/img";
            string imagePath = Path.Combine(imgPad, imageFileName);
            return imagePath;
        }

        #endregion
    }
}
