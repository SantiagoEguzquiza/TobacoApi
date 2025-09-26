﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TobacoBackend.Domain.IServices;
using TobacoBackend.DTOs;
using System.Text.RegularExpressions;

namespace TobacoBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        private readonly string[] _allowedColors = {
            "#FF8A00", // Orange
            "#3B82F6", // Blue
            "#10B981", // Green
            "#F59E0B", // Amber
            "#EF4444", // Red
            "#8B5CF6", // Purple
            "#06B6D4", // Cyan
            "#9E9E9E"  // Gray (default)
        };

        private bool IsValidColor(string colorHex)
        {
            if (string.IsNullOrEmpty(colorHex))
                return false;

            // Check if it's a valid hex color format
            if (!Regex.IsMatch(colorHex, @"^#[0-9A-Fa-f]{6}$"))
                return false;

            // Check if it's in the allowed colors list
            return _allowedColors.Contains(colorHex.ToUpper());
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoriaDTO>>> GetAllCategorias()
        {
            var categorias = await _categoriaService.GetAllAsync();
            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDTO>> GetCategoriaById(int id)
        {
            try
            {
                var categoria = await _categoriaService.GetByIdAsync(id);
                if (categoria == null)
                    return NotFound(new { message = "No se encontró la categoría con el ID proporcionado." });

                return Ok(categoria);
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Ocurrió un error al buscar la categoría." });
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddCategoria([FromBody] CategoriaDTO categoriaDto)
        {
            try
            {
                if (categoriaDto == null)
                    return BadRequest(new { message = "La categoría no puede ser nula." });

                if (string.IsNullOrEmpty(categoriaDto.Nombre))
                    return BadRequest(new { message = "El nombre de la categoría es requerido." });

                // Set default color if not provided or invalid
                if (string.IsNullOrEmpty(categoriaDto.ColorHex) || !IsValidColor(categoriaDto.ColorHex))
                {
                    categoriaDto.ColorHex = "#9E9E9E"; // Default gray
                }

                await _categoriaService.AddAsync(categoriaDto);
                return Ok(new { message = "Categoría agregada exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al agregar la categoría: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCategoria(int id, [FromBody] CategoriaDTO categoriaDto)
        {
            if (categoriaDto == null || id != categoriaDto.Id)
                return BadRequest(new { message = "ID de la categoría no coincide o la categoría es nula." });

            if (string.IsNullOrEmpty(categoriaDto.Nombre))
                return BadRequest(new { message = "El nombre de la categoría es requerido." });

            // Validate color if provided
            if (!string.IsNullOrEmpty(categoriaDto.ColorHex) && !IsValidColor(categoriaDto.ColorHex))
                return BadRequest(new { message = "El color seleccionado no está permitido. Use uno de los colores de la paleta." });

            // Set default color if not provided
            if (string.IsNullOrEmpty(categoriaDto.ColorHex))
            {
                categoriaDto.ColorHex = "#9E9E9E"; // Default gray
            }

            try
            {
                await _categoriaService.UpdateAsync(id, categoriaDto);
                return Ok(new { message = "Categoría actualizada exitosamente." });
            }
            catch (Exception)
            {
                return NotFound(new { message = "Categoría no encontrada." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategoria(int id)
        {
            try
            {
                await _categoriaService.DeleteAsync(id);
                return Ok(new { message = "Categoría eliminada exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al eliminar la categoría: {ex.Message}" });
            }
        }
    }
}
