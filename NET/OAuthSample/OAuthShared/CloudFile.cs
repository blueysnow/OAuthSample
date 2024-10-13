namespace OAuthShared;

public class CloudFile(string id = "", string name = "", DateTimeOffset? createdDate = null, long? size = null)
{
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public DateTime? CreatedDate { get; set; } = createdDate?.DateTime;
    public long? Size { get; set; } = size;
}
