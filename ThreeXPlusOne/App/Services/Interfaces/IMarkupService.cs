namespace ThreeXPlusOne.App.Services.Interfaces;

public interface IMarkupService : ISingletonService
{
    IEnumerable<string> ParseMarkup(string message);
}