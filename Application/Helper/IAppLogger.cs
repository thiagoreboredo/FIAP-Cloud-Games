namespace Application.Helper
{
    public interface IAppLogger<T>
    {
        void LogInformation(string message);
        void LogError(string message);
        void LogWarning(string message);
    }
}