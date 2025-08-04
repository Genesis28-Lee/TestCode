public interface IDbService : IDisposable
{
    /// <summary>
    /// 서비스가 초기화되었는지 여부를 가져온다.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// 서비스를 초기화 한다.
    /// </summary>
    void Initialize();

    IReadOnlyList<ITestItem> GetAllTestItem();
}
