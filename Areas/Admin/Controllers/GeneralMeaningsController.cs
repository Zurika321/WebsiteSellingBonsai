﻿using System;
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
    public class GeneralMeaningsController : Controller
    {
        private readonly MiniBonsaiDBAPI _context;

        public GeneralMeaningsController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: Admin/GeneralMeanings
        public async Task<IActionResult> Index()
        {
            return View(await _context.GeneralMeaning.ToListAsync());
        }

        // GET: Admin/GeneralMeanings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generalMeaning = await _context.GeneralMeaning
                .FirstOrDefaultAsync(m => m.Id == id);
            if (generalMeaning == null)
            {
                return NotFound();
            }

            return View(generalMeaning);
        }

        // GET: Admin/GeneralMeanings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/GeneralMeanings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Meaning,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] GeneralMeaning generalMeaning)
        {
            if (ModelState.IsValid)
            {
                _context.Add(generalMeaning);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(generalMeaning);
        }

        // GET: Admin/GeneralMeanings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generalMeaning = await _context.GeneralMeaning.FindAsync(id);
            if (generalMeaning == null)
            {
                return NotFound();
            }
            return View(generalMeaning);
        }

        // POST: Admin/GeneralMeanings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Meaning,CreatedDate,CreatedBy,UpdatedDate,UpdatedBy")] GeneralMeaning generalMeaning)
        {
            if (id != generalMeaning.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(generalMeaning);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeneralMeaningExists(generalMeaning.Id))
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
            return View(generalMeaning);
        }

        // GET: Admin/GeneralMeanings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generalMeaning = await _context.GeneralMeaning
                .FirstOrDefaultAsync(m => m.Id == id);
            if (generalMeaning == null)
            {
                return NotFound();
            }

            return View(generalMeaning);
        }

        // POST: Admin/GeneralMeanings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id,string password)
        {
            var generalMeaning = await _context.GeneralMeaning
                .Include(g => g.Bonsais)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (generalMeaning != null)
            {
                if (generalMeaning.Bonsais.Any())
                {
                    if (string.IsNullOrEmpty(password))
                    {
                        ViewData["ErrorMessage"] = "Có ràng buộc với các Bonsais liên quan. Nhập mật khẩu để xác nhận xóa.";
                        return View("ConfirmDelete", generalMeaning);
                    }

                    if (password != "123456") 
                    {
                        ViewData["ErrorMessage"] = "Mật khẩu không đúng. Vui lòng thử lại.";
                        return View("ConfirmDelete", generalMeaning);
                    }

                    _context.Bonsais.RemoveRange(generalMeaning.Bonsais);
                }

                _context.GeneralMeaning.Remove(generalMeaning);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool GeneralMeaningExists(int id)
        {
            return _context.GeneralMeaning.Any(e => e.Id == id);
        }
        
        public async Task<IActionResult> ConfirmDelete(GeneralMeaning generalMeaning)
        {
            return View(generalMeaning);
        }
    }
     
}