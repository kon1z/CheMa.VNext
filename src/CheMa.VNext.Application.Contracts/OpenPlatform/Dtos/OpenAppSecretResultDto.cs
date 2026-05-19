namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenAppSecretResultDto
{
    public OpenAppDto OpenApp { get; set; } = new();

    public string AppSecret { get; set; } = string.Empty;
}
