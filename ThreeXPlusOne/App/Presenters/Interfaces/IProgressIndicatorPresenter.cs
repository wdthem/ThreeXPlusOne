namespace ThreeXPlusOne.App.Presenters.Interfaces;

public interface IProgressIndicatorPresenter
{
    Task StartSpinningBar(string? message = null);
    Task StopSpinningBar(string? message = null);
}