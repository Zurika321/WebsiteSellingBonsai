using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.DTOS;
using System.Net.Http;

namespace WebsiteSellingBonsaiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BonsaisController : ControllerBase
    {
        private readonly MiniBonsaiDBAPI _context;

        public BonsaisController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: api/Bonsais
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BonsaiDTO>>> GetBonsais()
        {
            // Lấy tất cả dữ liệu từ bảng Bonsais
            var bonsais = await _context.Bonsais.Include(b => b.Type)
                                                .Include(b => b.Style)
                                                .Include(b => b.GeneralMeaning)
                                                .ToListAsync();

            // Map dữ liệu từ Bonsai sang BonsaiDTO
            var bonsaiDTOs = bonsais.Select(bonsai => new BonsaiDTO
            {
                Id = bonsai.Id,
                BonsaiName = bonsai.BonsaiName,
                Description = bonsai.Description,
                FengShuiMeaning = bonsai.FengShuiMeaning,
                Size = bonsai.Size,
                YearOld = bonsai.YearOld,
                MinLife = bonsai.MinLife,
                MaxLife = bonsai.MaxLife,
                Price = bonsai.Price,
                Quantity = bonsai.Quantity,
                ImageOld = bonsai.Image, // Gán Image cũ từ Bonsai
                nopwr = bonsai.NOPWR,
                rates = bonsai.Rates,
                TypeId = bonsai.TypeId,
                Type = new BonsaiType
                {
                    Id = bonsai.Type.Id,
                    Name = bonsai.Type.Name
                },
                StyleId = bonsai.StyleId,
                Style = new Style
                {
                    Id = bonsai.Style.Id,
                    Name = bonsai.Style.Name
                },
                GeneralMeaningId = bonsai.GeneralMeaningId,
                GeneralMeaning = new GeneralMeaning
                {
                    Id = bonsai.GeneralMeaning.Id,
                    Meaning = bonsai.GeneralMeaning.Meaning
                },
                //CreatedBy = bonsai.CreatedBy,
                //CreatedDate = bonsai.CreatedDate,
                //UpdatedBy = bonsai.UpdatedBy,
                //UpdatedDate = bonsai.UpdatedDate,
            }).ToList();

            return Ok(bonsaiDTOs); // Trả về danh sách DTO
        }


        // GET: api/Bonsais/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BonsaiDTO>> GetBonsai(int id)
        {
            // Sử dụng Include để bao gồm các bảng liên quan
            var bonsai = await _context.Bonsais
                .Include(b => b.Type)  // Bao gồm BonsaiType
                .Include(b => b.Style) // Bao gồm Style
                .Include(b => b.GeneralMeaning) // Bao gồm GeneralMeaning
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bonsai == null)
            {
                return NotFound();
            }

            var bonsaiDTO = new BonsaiDTO
            {
                Id = bonsai.Id,
                BonsaiName = bonsai.BonsaiName,
                Description = bonsai.Description,
                FengShuiMeaning = bonsai.FengShuiMeaning,
                Size = bonsai.Size,
                YearOld = bonsai.YearOld,
                MinLife = bonsai.MinLife,
                MaxLife = bonsai.MaxLife,
                Price = bonsai.Price,
                Quantity = bonsai.Quantity,
                ImageOld = bonsai.Image,
                nopwr = bonsai.NOPWR,
                rates = bonsai.Rates,
                TypeId = bonsai.TypeId,
                Type = bonsai.Type != null ? new BonsaiType
                {
                    Id = bonsai.Type.Id,
                    Name = bonsai.Type.Name
                } : null,
                StyleId = bonsai.StyleId,
                Style = bonsai.Style != null ? new Style
                {
                    Id = bonsai.Style.Id,
                    Name = bonsai.Style.Name
                } : null,
                GeneralMeaningId = bonsai.GeneralMeaningId,
                GeneralMeaning = bonsai.GeneralMeaning != null ? new GeneralMeaning
                {
                    Id = bonsai.GeneralMeaning.Id,
                    Meaning = bonsai.GeneralMeaning.Meaning
                } : null,
            };

            return bonsaiDTO;
        }


        // PUT: api/Bonsais/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBonsai(int id, Bonsai bonsai)
        {
            if (id != bonsai.Id)
            {
                return BadRequest();
            }

            _context.Entry(bonsai).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BonsaiExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Bonsais
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Bonsai>> PostBonsai(Bonsai bonsai)
        {
            _context.Bonsais.Add(bonsai);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBonsai", new { id = bonsai.Id }, bonsai);
        }

        // DELETE: api/Bonsais/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBonsai(int id)
        {
            var bonsai = await _context.Bonsais.FindAsync(id);
            if (bonsai == null)
            {
                return NotFound();
            }

            _context.Bonsais.Remove(bonsai);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BonsaiExists(int id)
        {
            return _context.Bonsais.Any(e => e.Id == id);
        }
    }
}
