namespace kv_dataloader_csharp.models;

public class HuggingFaceResponse
{
    public float[] embedding { get; set; } = Array.Empty<float>();
    public int dim { get; set; } = 0;
    public string model { get; set; } = string.Empty;
    public bool trust_remote_code { get; set; } = false;
    public bool predefined { get; set; } = false;
}