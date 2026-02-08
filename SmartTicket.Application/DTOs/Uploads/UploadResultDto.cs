namespace SmartTicket.Application.DTOs.Uploads;

public record UploadResultDto(
    string Url,
    string FileName,
    long Size,
    string ContentType
);
