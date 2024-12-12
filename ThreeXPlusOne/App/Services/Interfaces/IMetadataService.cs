using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Services.Interfaces;

public interface IMetadataService : IScopedService
{
    /// <summary>
    /// Generate the metadata and histogram based on the lists of series numbers.
    /// </summary>
    /// <param name="collatzResults"></param>
    Task GenerateMetadata(List<CollatzResult> collatzResults);
}
