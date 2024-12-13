namespace ThreeXPlusOne.App.Presenters.Interfaces;

public interface IProgressIndicatorPresenter
{
    Task StartSpinner(string? message = null);
    Task StopSpinner();
}