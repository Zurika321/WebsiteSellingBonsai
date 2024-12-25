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
    public class TypesController : Controller
    {
        private readonly MiniBonsaiDBAPI _context;

        public TypesController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: Admin/Types
        public async Task<IActionResult> Index()
        {
            return View(await _context.Types.ToListAsync());
        }

        // GET: Admin/Types/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonsaiType = await _context.Types
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bonsaiType == null)
            {
                return NotFound();
            }

            return View(bonsaiType);
        }

        // GET: Admin/Types/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Types/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] BonsaiType bonsaiType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bonsaiType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bonsaiType);
        }

        // GET: Admin/Types/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonsaiType = await _context.Types.FindAsync(id);
            if (bonsaiType == null)
            {
                return NotFound();
            }
            return View(bonsaiType);
        }

        // POST: Admin/Types/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] BonsaiType bonsaiType)
        {
            if (id != bonsaiType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bonsaiType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BonsaiTypeExists(bonsaiType.Id))
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
            return View(bonsaiType);
        }

        // GET: Admin/Types/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonsaiType = await _context.Types
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bonsaiType == null)
            {
                return NotFound();
            }

            return View(bonsaiType);
        }

        // POST: Admin/Types/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bonsaiType = await _context.Types
                            .Include(t => t.Bonsais)
                            .FirstOrDefaultAsync(t => t.Id == id);

            if (bonsaiType != null)
            {
                _context.Bonsais.RemoveRange(bonsaiType.Bonsais);

                _context.Types.Remove(bonsaiType);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BonsaiTypeExists(int id)
        {
            return _context.Types.Any(e => e.Id == id);
        }
    }
}
