using System.Collections.Generic;

namespace TerraDotnet.TerraLcd.Messages;

public class LcdError
{
    public int Code { get; set; }

    public string Message { get; set; } = string.Empty;

    public List<string> Details { get; set; } = new();
}