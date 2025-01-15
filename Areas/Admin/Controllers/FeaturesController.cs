using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.DTOS.View;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsai.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FeaturesController : Controller
    {
        private readonly MiniBonsaiDBAPI _context;
        private readonly APIServices _apiServices;

        public FeaturesController(MiniBonsaiDBAPI context, APIServices apiServices)
        {
            _context = context;
            _apiServices = apiServices;
        }
        private string GetUrl()
        {
            var url = _apiServices.GetUrl();
            return url.Substring(0, url.Length - 4);
        }

        // GET: Admin/Features
        public async Task<IActionResult> Index()
        {
            return View(await _context.Features.ToListAsync());
        }

        // GET: Admin/Features/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feature = await _context.Features
                .FirstOrDefaultAsync(m => m.FEA_ID == id);
            if (feature == null)
            {
                return NotFound();
            }

            return View(feature);
        }

        // GET: Admin/Features/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Features/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] FeatureDTO feature)
        {
            if (ModelState.IsValid)
            {
                var url =  GetUrl();

                if (!feature.Link.StartsWith(url))
                {
                    TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                    {
                        Message = $"Đường link phải bắt đầu là: {url}",
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 8,
                    });
                    return View(feature);
                }
                var AvatarPath = await _apiServices.ProcessImage(feature.ImageFile, feature.ImageUrl, "Feature");

                var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
                if (userInfo == null)
                    return RedirectToAction("Login", "Users", new { area = "Admin" });
                var featurenew = new Feature
                {
                    FEA_ID = feature.FEA_ID,
                    Title = feature.Title,
                    ImageUrl = AvatarPath,
                    Description = feature.Description,
                    Link = feature.Link,
                    CreatedBy = userInfo.UserName,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = userInfo.UserName,
                    UpdatedDate = DateTime.Now,
                };
                _context.Add(featurenew);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(feature);
        }

        // GET: Admin/Features/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feature = await _context.Features.FindAsync(id);

            if (feature == null)
            {
                return NotFound();
            }
            var featuredto = new FeatureDTO
            {
                FEA_ID = feature.FEA_ID,
                Title = feature.Title,
                ImageUrl = feature.ImageUrl,
                Description = feature.Description,
                Link = feature.Link,
                CreatedBy = feature.CreatedBy,
                CreatedDate = feature.CreatedDate,
                UpdatedBy = feature.UpdatedBy,
                UpdatedDate = feature.UpdatedDate,
            };
            return View(featuredto);
        }

        // POST: Admin/Features/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] FeatureDTO feature)
        {
            if (id != feature.FEA_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var url = GetUrl();

                    if (!feature.Link.StartsWith(url))
                    {
                        TempData["ThongBao"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ThongBao
                        {
                            Message = $"Đường link phải bắt đầu là: {url}",
                            MessageType = TypeThongBao.Warning,
                            DisplayTime = 8,
                        });
                        return View(feature);
                    }
                    var AvatarPath = await _apiServices.ProcessImage(feature.ImageFile, feature.ImageUrl, "Feature");
                    var userInfo = HttpContext.Session.Get<ApplicationUserDTO>("userInfo");
                    if (userInfo == null)
                        return RedirectToAction("Login", "Users", new { area = "Admin" });
                    var featurenew = new Feature
                    {
                        FEA_ID = feature.FEA_ID,
                        Title = feature.Title,
                        ImageUrl = AvatarPath,
                        Description = feature.Description,
                        Link = feature.Link,
                        CreatedBy = feature.CreatedBy,
                        CreatedDate = feature.CreatedDate,
                        UpdatedBy = userInfo.UserName,
                        UpdatedDate = DateTime.Now,
                    };
                    _context.Update(featurenew);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeatureExists(feature.FEA_ID))
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
            return View(feature);
        }

        // GET: Admin/Features/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feature = await _context.Features
                .FirstOrDefaultAsync(m => m.FEA_ID == id);
            if (feature == null)
            {
                return NotFound();
            }

            return View(feature);
        }

        // POST: Admin/Features/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feature = await _context.Features.FindAsync(id);
            if (feature != null)
            {
                _context.Features.Remove(feature);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeatureExists(int id)
        {
            return _context.Features.Any(e => e.FEA_ID == id);
        }
    }
}
