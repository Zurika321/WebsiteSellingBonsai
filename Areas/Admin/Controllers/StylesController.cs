using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StylesController : Controller
    {
        private readonly MiniBonsaiDBAPI _context;

        public StylesController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: Admin/Styles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Styles.ToListAsync());
        }

        // GET: Admin/Styles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var style = await _context.Styles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (style == null)
            {
                return NotFound();
            }

            return View(style);
        }

        // GET: Admin/Styles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Styles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] Style style)
        {
            if (ModelState.IsValid)
            {
                _context.Add(style);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(style);
        }

        // GET: Admin/Styles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var style = await _context.Styles.FindAsync(id);
            if (style == null)
            {
                return NotFound();
            }
            return View(style);
        }

        // POST: Admin/Styles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] Style style)
        {
            if (id != style.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(style);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StyleExists(style.Id))
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
            return View(style);
        }

        // GET: Admin/Styles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var style = await _context.Styles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (style == null)
            {
                return NotFound();
            }

            return View(style);
        }

        // POST: Admin/Styles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var style = await _context.Styles
                        .Include(s => s.Bonsais)
                        .FirstOrDefaultAsync(s => s.Id == id);

            if (style != null)
            {
                _context.Bonsais.RemoveRange(style.Bonsais);

                _context.Styles.Remove(style);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StyleExists(int id)
        {
            return _context.Styles.Any(e => e.Id == id);
        }
    }
}
