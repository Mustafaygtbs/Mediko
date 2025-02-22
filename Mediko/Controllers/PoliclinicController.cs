using Mediko.DataAccess.Interfaces;
using Mediko.DataAccess;
using Mediko.Entities;
using Mediko.Entities.DTOs.PoliclinicDTOs;
using Mediko.Entities.Exceptions;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Mediko.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles ="Admin")]
    public class PoliclinicController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MedikoDbContext _context;
        private readonly IMapper _mapper;

        public PoliclinicController(IUnitOfWork unitOfWork, MedikoDbContext context, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            
            try
            {
                var policlinics = await _unitOfWork.PoliclinicRepository.GetAllAsync();
                foreach (var pol in policlinics)
                {
                    await _context.Entry(pol).Reference(p => p.Department).LoadAsync();
                }
                var policlinicDtos = _mapper.Map<IEnumerable<PoliclinicDto>>(policlinics);
                return Ok(policlinicDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        [HttpGet("GetByIdWithDepartment/{id}")]
        public async Task<IActionResult> GetByIdWithDepartment(int id)
        {
            try
            {
                var policlinic = await _unitOfWork.PoliclinicRepository.GetByIdAsync(id);
                await _context.Entry(policlinic).Reference(p => p.Department).LoadAsync();
                var policlinicDto = _mapper.Map<PoliclinicDto>(policlinic);
                return Ok(policlinicDto);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        [HttpGet("GetByIdOnlyPoliclinic/{id}")]
        public async Task<IActionResult> GetPoliclinicOnly(int id)
        {
            try
            {
                var policlinic = await _unitOfWork.PoliclinicRepository.GetByIdAsync(id);
                var policlinicDto = _mapper.Map<PoliclinicDto>(policlinic);
                policlinicDto.Department = null;
                return Ok(policlinicDto);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] PoliclinicCreateDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Poliklinik verisi geçersiz.");

                var entity = _mapper.Map<Policlinic>(model);
                await _unitOfWork.PoliclinicRepository.AddAsync(entity);
                await _unitOfWork.Save();
                return CreatedAtAction(nameof(GetByIdWithDepartment), new { id = entity.Id }, entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        [HttpPost("CreateWithName")]
        public async Task<IActionResult> CreateWithName([FromBody] PoliclinicCreateWithNameDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Poliklinik verisi geçersiz.");

                var department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == model.DepartmentName);
                if (department == null)
                    return NotFound($"'{model.DepartmentName}' adına sahip bir Department bulunamadı.");

                var entity = _mapper.Map<Policlinic>(model);
                entity.DepartmentId = department.Id;
                await _unitOfWork.PoliclinicRepository.AddAsync(entity);
                await _unitOfWork.Save();
                return CreatedAtAction(nameof(GetByIdWithDepartment), new { id = entity.Id }, entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PoliclinicUpdateDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Poliklinik verisi geçersiz.");

                var existing = await _unitOfWork.PoliclinicRepository.GetByIdAsync(id);
                _mapper.Map(model, existing);
                _unitOfWork.PoliclinicRepository.Update(existing);
                await _unitOfWork.Save();
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _unitOfWork.PoliclinicRepository.GetByIdAsync(id);
                _unitOfWork.PoliclinicRepository.Delete(existing);
                await _unitOfWork.Save();
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }
    }
}
