using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos;
using HospitalManagementSystem.Dtos.Department;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Repository.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentsRepository _repo;

        public DepartmentController(IDepartmentsRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _repo.GetAllAsync();
            return View(departments);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(DepartmentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _repo.CreateAsync(dto);
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Error creating department.");
                return View(dto);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var department = await _repo.GetByIdAsync(id);
            if (department == null) return NotFound();

            var dto = new DepartmentDto
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name
            };

            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(DepartmentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                if (!await _repo.UpdateAsync(dto)) return NotFound();
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Error updating department.");
                return View(dto);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDepartment(int departmentId)
        {
            try
            {
                if (!await _repo.DeleteAsync(departmentId)) return NotFound();
                return RedirectToAction("Index");
            }
            catch
            {
                return StatusCode(500, "Error deleting department.");
            }
        }



    }
}
