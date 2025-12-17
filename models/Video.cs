using Newtonsoft.Json;
using Cassandra;

namespace kv_dataloader_csharp.models;

public class Video
{
    [JsonProperty("video_id")]
    public Guid videoId { get; set; } = Guid.NewGuid();
    [JsonProperty("user_id")]
    public Guid userId { get; set; } = Guid.NewGuid();
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string location { get; set; } = string.Empty;
    [JsonProperty("location_type")]
    public int locationType { get; set; } = 0;
    [JsonProperty("preview_image_location")]
    public string previewImageLocation { get; set; } = string.Empty;
    [JsonProperty("content_features")]
    public CqlVector<float>? contentFeatures { get; set; }
    [JsonProperty("added_date")]
    public DateTime addedDate { get; set; } = DateTime.UtcNow;
    public HashSet<string> tags { get; set; } = new();
    public int views { get; set; } = 0;
    [JsonProperty("youtube_id")]
    public string youtubeId { get; set; } = string.Empty;
    [JsonProperty("content_rating")]
    public string contentRating { get; set; } = string.Empty;
    public string category { get; set; } = string.Empty;
    public string language { get; set; } = string.Empty;
}
