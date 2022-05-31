using Hoyo.Enums;

namespace Hoyo.OcrServer;
public class PortraitInfo
{
    public string Name { get; set; } = string.Empty;
    public EGender Gender { get; set; } = EGender.男;
    public ENation Nation { get; set; } = ENation.汉族;
    public DateOnly Birthday { get; set; }
    public string Address { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
}

public class EmblemInfo
{
    public string Agency { get; set; } = string.Empty;
    public string ValidDateBegin { get; set; } = string.Empty;
    public string ValidDateEnd { get; set; } = string.Empty;
}