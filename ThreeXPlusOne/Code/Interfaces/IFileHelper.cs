namespace ThreeXPlusOne.Code.Interfaces;

public interface IFileHelper
{
    void WriteMetadataToFile(string content, string filePath);
    string GenerateDirectedGraphFilePath();
    string GenerateHistogramFilePath();
    string GenerateMetadataFilePath();
}