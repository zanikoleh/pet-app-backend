namespace NotificationService.Application.DTOs;

/// <summary>
/// DTO for email notification.
/// </summary>
public class EmailNotificationDto
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? PlainTextContent { get; set; }
    public DateTime SentAt { get; set; }
}
