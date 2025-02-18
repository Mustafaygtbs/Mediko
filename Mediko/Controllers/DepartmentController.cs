using Mediko.DataAccess.Interfaces;
using Mediko.DataAccess;
using Mediko.DataAccess.Repositories; // IUnitOfWork, GenericRepository vs.
using Mediko.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mediko.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MedikoDbContext _context;

        public DepartmentController(IUnitOfWork unitOfWork, MedikoDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // GET: api/Department/Get-All
        [HttpGet("Get-All")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Temel Department verilerini çekiyoruz
                var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();

                // Her Department için explicit loading ile bağlı Policlinics koleksiyonunu yüklüyoruz
                foreach (var dept in departments)
                {
                    await _context.Entry(dept).Collection(d => d.Policlinics).LoadAsync();
                }

                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        // GET: api/Department/GetById/{id}
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // ID'ye göre department bul
                var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
                if (department == null)
                    return NotFound($"Department with Id={id} not found.");

                // Explicit loading: Bağlı Policlinics koleksiyonunu yükle
                await _context.Entry(department).Collection(d => d.Policlinics).LoadAsync();

                return Ok(department);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        // POST: api/Department/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Department model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Department data is invalid.");

                await _unitOfWork.DepartmentRepository.AddAsync(model);
                await _unitOfWork.Save();

                return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
            }
            catch (ArgumentNullException ane)
            {
                return BadRequest(ane.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        // PUT: api/Department/Update/{id}
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Department model)
        {
            try
            {
                if (model == null || id != model.Id)
                    return BadRequest("Department data is invalid or ID mismatch.");

                var existing = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound($"Department with Id={id} not found.");

                // Gerekli alanları güncelle (örneğin Name)
                existing.Name = model.Name;
                // Eğer başka alanlar varsa güncelleyebilirsin

                _unitOfWork.DepartmentRepository.Update(existing);
                await _unitOfWork.Save();

                return NoContent();
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        // DELETE: api/Department/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
                if (existing == null)
                    return NotFound($"Department with Id={id} not found.");

                _unitOfWork.DepartmentRepository.Delete(existing);
                await _unitOfWork.Save();

                return NoContent();
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }
    }
}
