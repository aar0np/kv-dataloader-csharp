using Cassandra.Mapping;
using kv_dataloader_csharp.models;

public class MappingHelper : Mappings
{
    public MappingHelper()
    {
        For<Video>()
            .TableName("videos")
            .PartitionKey("videoid")
            .Column(v => v.addedDate, cm => cm.WithName("added_date"))
            .Column(v => v.contentFeatures, cm => cm.WithName("content_features"))
            .Column(v => v.locationType, cm => cm.WithName("location_type"))
            .Column(v => v.previewImageLocation, cm => cm.WithName("preview_image_location"))
            .Column(v => v.youtubeId, cm => cm.WithName("youtube_id"))
            .Column(v => v.contentRating, cm => cm.WithName("content_rating"));
    }
}