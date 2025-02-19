using Mediko.DataAccess.Interfaces;
using Mediko.DataAccess;
using Mediko.Entities;
using Mediko.Entities.DTOs.DepartmentDTOs;
using Mediko.Entities.Exceptions;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace Mediko.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MedikoDbContext _context;
        private readonly IMapper _mapper;

        public DepartmentController(IUnitOfWork unitOfWork, MedikoDbContext context, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("Get-All")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
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

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
                return Ok(department);
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
        public async Task<IActionResult> Create([FromBody] DepartmentCreateDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Department data is invalid.");

                var departmentEntity = _mapper.Map<Department>(model);
                await _unitOfWork.DepartmentRepository.AddAsync(departmentEntity);
                await _unitOfWork.Save();

                return CreatedAtAction(nameof(GetById), new { id = departmentEntity.Id }, departmentEntity);
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

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentUpdateDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest("Department data is invalid.");

                var existingDepartment = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
                _mapper.Map(model, existingDepartment);

                _unitOfWork.DepartmentRepository.Update(existingDepartment);
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
                var existingDepartment = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
                _unitOfWork.DepartmentRepository.Delete(existingDepartment);
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
