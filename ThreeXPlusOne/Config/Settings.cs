
namespace ThreeXPlusOne.Config;

public class Settings
{
    public int CanvasWidth { get; set; }
    public int CanvasHeight { get; set; }
    public int NumberOfSeries { get; set; }
    public int MaxStartingNumber { get; set; }
    public double RotationAngle { get; set; }
    public int XNodeSpacer { get; set; }
    public int YNodeSpacer { get; set; }
    public bool GenerateImage { get; set; }
    public string? ImagePath { get; set; }
}