namespace GardenToolSharing.Api.Models;

public class Notice
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<NoticeComment> Comments { get; set; } = [];
}

public class NoticeComment
{
    public int Id { get; set; }
    public int NoticeId { get; set; }
    public Notice Notice { get; set; } = null!;

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}