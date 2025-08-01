namespace Plus;

public interface IPlusEnvironment
{
    Task<bool> Start();
    void PerformShutDown();
}