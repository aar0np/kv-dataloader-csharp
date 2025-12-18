using Cassandra;
using Cassandra.Mapping;
using CsvHelper;
using kv_dataloader_csharp.models;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public class KVDataLoader
{
    private static readonly string? _dataDir = System.Environment.GetEnvironmentVariable("DATA_DIR");
    private static readonly string? _HF_API_KEY = System.Environment.GetEnvironmentVariable("HF_API_KEY");
    private static readonly string _HF_APLOETZ_SPACE_ENDPOINT = "https://aploetz-granite-embeddings.hf.space/embed";
    private static readonly string _modelId = "ibm-granite/granite-embedding-30m-english";
    private static List<string> _YOUTUBE_PATTERNS = new List<string>();
    private static HttpClient _hFHttpClient = new HttpClient();

    public static async Task Main(string[] args)
    {
        if (string.IsNullOrEmpty(_HF_API_KEY))
        {
            Console.WriteLine("ERROR: HF_API_KEY must be defined as an environment variable.");
        }

        // connect to Astra DB
        ISession session = GetCQLSession();
        IMapper mapper = new Mapper(session);

        // Regex patterns to get the YouTubeID from the location
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtu\\.be/(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/watch\\?v=(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/embed/(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/v/(?<id>[A-Za-z0-9_-]{11})");
        _YOUTUBE_PATTERNS.Add("(?:https?://)?(?:www\\.)?youtube\\.com/shorts/(?<id>[A-Za-z0-9_-]{11})");

        int numRecords = 0;

        // load csv file
        using (var reader = new StreamReader(_dataDir + "videos.csv"))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            // read each line
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                Video video = new Video() {
                    videoId = csv.GetField<Guid>("videoid"),
                    userId = csv.GetField<Guid>("userid"),
                    addedDate = csv.GetField<DateTime>("added_date"),
                    name = csv.GetField<string>("name"),
                    description = csv.GetField<string>("description"),
                    location = csv.GetField<string>("location"),
                    previewImageLocation = csv.GetField<string>("preview_image_location"),
                    contentRating = csv.GetField<string>("content_rating"),
                    category = csv.GetField<string>("category"),
                    language = csv.GetField<string>("language")
                };

                // get the YouTubeId
                video.youtubeId = extractYouTubeId(video.location);

                // generate embeddings
                var request = new HuggingFaceRequest();
                request.text = video.name;
                request.model = _modelId;

                string jsonResponse = await getEmbeddings(request);
                
                HuggingFaceResponse hFResp = JsonConvert.DeserializeObject<HuggingFaceResponse>(jsonResponse);

                video.contentFeatures = (CqlVector<float>)hFResp.embedding;

                // write to Astra DB
                mapper.Insert(video);

                // write video name and vector embedding to console
                Console.WriteLine(video.name + " " + video.youtubeId + " " + video.contentFeatures);
                numRecords++;
            }
        }

        Console.WriteLine($"Data load into Astra DB complete at {numRecords} records");
    }

    private static async Task<string> getEmbeddings(HuggingFaceRequest req)
    {
        var json = JsonConvert.SerializeObject(req);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var hFRequestMessage = new HttpRequestMessage(HttpMethod.Post, _HF_APLOETZ_SPACE_ENDPOINT)
        {
            Content = data
        };
        HttpResponseMessage hFResponse = await _hFHttpClient.SendAsync(hFRequestMessage);
        return await hFResponse.Content.ReadAsStringAsync();
    }

    private static string extractYouTubeId(string youtubeUrl)
    {

        foreach (string pattern in _YOUTUBE_PATTERNS)
        {
            MatchCollection matches = Regex.Matches(youtubeUrl, pattern);

            if (matches.Any())
            {
                Match match = matches.First();
                GroupCollection group = match.Groups;
                return group["id"].ToString();
            }
        }
        return string.Empty;
    }

    private static ISession GetCQLSession()
    {
        string? _astraDbApplicationToken = System.Environment.GetEnvironmentVariable("ASTRA_DB_APPLICATION_TOKEN");
        string? _astraDbKeyspace = System.Environment.GetEnvironmentVariable("ASTRA_DB_KEYSPACE");
        string? _secureBundleLocation = System.Environment.GetEnvironmentVariable("ASTRA_DB_SECURE_BUNDLE_LOCATION");
        MappingConfiguration.Global.Define<MappingHelper>();

        ISession session =
            Cluster.Builder()
                   .WithCloudSecureConnectionBundle(_secureBundleLocation)
                   .WithCredentials("token", _astraDbApplicationToken)
                   .WithDefaultKeyspace(_astraDbKeyspace)
                   .Build()
                   .Connect();

        return session;
    }
}
