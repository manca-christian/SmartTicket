using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.Application.DTOs.Uploads;

namespace SmartTicket.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/uploads")]
[Route("api/uploads")]
[Authorize]
public sealed class UploadsController : ControllerBase
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/webp"
    };

    private readonly IWebHostEnvironment _environment;

    public UploadsController(IWebHostEnvironment environment) => _environment = environment;

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(List<UploadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload([FromForm] IFormFileCollection files)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { message = "Nessun file ricevuto." });

        if (files.Count > 5)
            return BadRequest(new { message = "Massimo 5 file." });

        var uploadsRoot = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var results = new List<UploadResultDto>();

        foreach (var file in files)
        {
            if (!AllowedContentTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Tipo file non consentito." });

            if (file.Length > 4 * 1024 * 1024)
                return BadRequest(new { message = "File troppo grande (max 4MB)." });

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            var url = $"/uploads/{fileName}";
            results.Add(new UploadResultDto(url, file.FileName, file.Length, file.ContentType));
        }

        return Ok(results);
    }
}
