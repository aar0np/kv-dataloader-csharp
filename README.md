# kv-dataloader-csharp
A simple data loader for the KillrVideo videos table, written in C# (.NET 9.0). This loader uses version 3.22.0 of the [CassandraCSharpDriver](https://www.nuget.org/packages/cassandracsharpdriver/) to write to Astra DB, and generates vector emebeddings using the `ibm-granite/granite-embedding-30m-english` model from HuggingFace.

## Prerequisites
1. **Dotnet 9+** runtime.
2. A **DataStax Astra DB** serverless database – [get a free account](https://astra.datastax.com).
3. The [videos.csv](https://github.com/KillrVideo/killrvideo-data/blob/master/data/csv/videos.csv) file from the [killrvideo/killrvideo-data](https://github.com/KillrVideo/killrvideo-data) repository.
4. A HuggingFace API key.

## Setup & Configuration
```bash
# clone
git clone git@github.com:KillrVideo/kv-dataloader-csharp.git
cd kv-dataloader-csharp

# build and install dependencies
dotnet build
```

Database schema:
1. Assumes that Astra DB has a keyspace named `killrvideo`. (_create it if it does not exist_)
2. Create the `videos` table from the CQL file in the [killrvideo/killrvideo-data](https://github.com/KillrVideo/killrvideo-data): <https://github.com/KillrVideo/killrvideo-data/blob/master/schema-astra.cql>
For reference, the table and its required index on the vector column can be seen here:
```sql
CREATE TABLE killrvideo.videos (
    videoid uuid PRIMARY KEY,
    added_date timestamp,
    category text,
    content_features vector<float, 384>,
    content_rating text,
    description text,
    language text,
    location text,
    location_type int,
    name text,
    preview_image_location text,
    tags set<text>,
    userid uuid,
    views int,
    youtube_id text);
CREATE CUSTOM INDEX videos_content_features_idx ON killrvideo.videos (content_features) USING 'StorageAttachedIndex' WITH OPTIONS = {'similarity_function': 'COSINE'};
```

Environment variables (via `export`):

| Variable | Description |
|----------|-------------|
| `ASTRA_DB_SECURE_BUNDLE_LOCATION` | Downloaded from the Astra UI once you have created your database |
| `ASTRA_DB_APPLICATION_TOKEN` | Token created in Astra UI |
| `ASTRA_DB_KEYSPACE` | `killrvideo` |

## Execution
```bash
dotnet build
dotnet run
```
Or simply...
```bash
dotnet run
```
