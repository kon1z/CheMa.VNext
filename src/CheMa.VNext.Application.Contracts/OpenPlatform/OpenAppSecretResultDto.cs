namespace CheMa.VNext.OpenPlatform;

public class OpenAppSecretResultDto
{
    public OpenAppDto OpenApp { get; set; } = new();

    public string AppSecret { get; set; } = string.Empty;
}
