using Commons.Utilities;

using EF.Data.DBContexts;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Prism.Events;

using System.Data;
using System.Runtime.CompilerServices;


namespace EF.Data.Services;


public static class RetryHelper
{
    public static async Task ExecuteWithRetry(Func<Task> action, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await action();
                Logger.WriteLog(LogTypes.Debug, $"RetryHelper ExecuteWithRetry count:{i}");
                return;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 8 && i < maxRetries - 1)
            {
                await Task.Delay(100 * (i + 1));  // 지수 백오프
            }
        }
        throw new Exception("Max retries exceeded");
    }
}

public class DbService : IDbService
{
    public bool IsInitialized { get; private set; }

    private readonly Func<DbContext> _contextFactory;
    private readonly BackgroundWriterService _writer;


    private IEventAggregator _IEventAggregator;
    public EventAggregator EventAggregator
    {
        get => (EventAggregator)_IEventAggregator;
    }



    #region Constructors & Initialize

    public DbService(Func<DbContext> contextFactory, BackgroundWriterService writer, IEventAggregator eventAggregator)
    {
        _contextFactory   = contextFactory;
        _writer           = writer;
        _IEventAggregator = eventAggregator;
    }

    public void Initialize()
    {
        using var context = _contextFactory();
        context.Database.Migrate();//DB Migration 해야될게 있을 때 사용

        IsInitialized = true;
    }

    public void Dispose()
    {
        _writer?.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion



    #region Methods

    /// <summary>
    /// Incubator들과 그것들에 포함된 TestItem을 가져온다.
    /// </summary>
    /// <returns>Incubator List (인큐베이터,테스트 정보)</returns>
    public async Task<IReadOnlyList<IIncubatorSlot>> GetAllIncubatorsWithTestItemAsync()
    {
        try
        {
            using var _DbContext = _contextFactory();
            // 변경된 db정보 갱신을 위해 초기화
            _DbContext.ChangeTracker.Clear();

            return await _DbContext.IncubatorSlots.Include(I => I.Panel)
                                                      .ThenInclude(P => P.TestItem)
                                                      .ThenInclude(TI => TI.TestOption)
                                                      .Include(I => I.Panel)
                                                      .ThenInclude(P => P.TestItem)
                                                      .ThenInclude(TI => TI.Sample)
                                                      .ThenInclude(S => S.Bacteria)
                                                      .Include(I => I.Panel)
                                                      .ThenInclude(P => P.TestItem)
                                                      .ThenInclude(TI => TI.Sample)
                                                      .ThenInclude(S => S.SampleSlot)
                                                      .Include(I => I.Panel)
                                                      .ThenInclude(P => P.dKit)
                                                      .Include(I => I.Panel)
                                                      .ThenInclude(P => P.TestItem)
                                                      .ThenInclude(TI => TI.TipCartridge)
                                                      .Include(I => I.Panel)
                                                      .ThenInclude(P => P.TestItem)
                                                      .ThenInclude(TI => TI.Sample)
                                                      .ThenInclude(S => S.SampleSlot)
                                                      .Include(I => I.Panel)
                                                      .ThenInclude(P => P.PanelSlot)
                                                      .OrderBy(incubator => incubator.SlotNumber)
                                                      .AsNoTracking()
                                                      .ToListAsync();
        }
        finally
        {
        }
    }

    public async Task<IReadOnlyList<ITestItem>> GetAllTestItemAsync()
    {
        try
        {
            using var _DbContext = _contextFactory();
            return await _DbContext.TestItems.AsNoTracking().ToListAsync();
        }
        finally
        {
        }
    }

    public async Task<ITestItem> GetTestItemAsync(int testItemId)
    {
        try
        {
            using var _DbContext = _contextFactory();
            var testItem = await _DbContext.TestItems.AsNoTracking().FirstAsync(item => item.Id == testItemId);
            return testItem;
        }
        catch (Exception ex)
        {
            if (ex.Message != null &&
                ex.Message?.Contains("A second operation was started on this context instance before a previous operation completed") == true)
            {
                var message = $"DbContext Exception GetTestItemAsync  ThreadId: {Environment.CurrentManagedThreadId}";
                EventAggregator?.GetEvent<DbServiceExceptionEvent>().Publish(new DbServiceExceptionEventArgs(message, ex));
            }
            return null;
        }
        finally
        {
        }
    }

    public IReadOnlyList<ITestItem> GetAllTestItem()
    {
        try
        {
            using var _DbContext = _contextFactory();
            return _DbContext.TestItems.AsNoTracking().ToList();
        }
        finally
        {
        }
    }

    /// <summary>
    /// 새로운 Test 등록
    /// </summary>
    /// <param name="gramType">Gram type</param>
    /// <param name="useQCTest">QC Test 여부</param>
    /// <param name="testStartPoint">Test 시작 위치</param>
    /// <param name="sampleId">Sample Id</param>
    /// <param name="tipCartridgeId">TipCartridge Id</param>
    /// <param name="panelId">Panel Id</param>
    /// <param name="userId">User Id</param>
    /// <returns>생성된 TestItem 반환</returns>
    public async Task<ITestItem> AddNewTestAsync(GramTypes gramType, bool useQCTest, TestStartActivity testStartPoint,
        int sampleId, int tipCartridgeId, int panelId, string userId)
    {
        try
        {
            var newTestItem = new TestItem
            {
                SampleId        = sampleId,
                TipCartridgeId  = tipCartridgeId,
                PanelId         = panelId,
                TestOption      = new TestOption
                {
                    GramType        = gramType,
                    UseMultiPanel   = false,
                    UseQCTest       = useQCTest,
                    TestStartActivity = testStartPoint,
                },
                TestState = TestStates.Regist,
                AccountId = userId
            };

            return await _writer.EnqueueWriteAsync(async context =>
            {
                context.TestItems.Add(newTestItem);
                await context.SaveChangesAsync();

                return newTestItem;
            });
        }
        catch (Exception ex)
        {
            if (ex.Message != null &&
                ex.Message?.Contains("A second operation was started on this context instance before a previous operation completed") == true)
            {
                var message = $"DbContext Exception AddNewTestAsync  ThreadId: {Environment.CurrentManagedThreadId}";
                EventAggregator?.GetEvent<DbServiceExceptionEvent>().Publish(new DbServiceExceptionEventArgs(message, ex));
            }
            return null;
        }
        finally
        {
        }
    }

    /// <summary>
    /// Test의 상태를 갱신
    /// </summary>
    /// <param name="testItemId">Test Id</param>
    /// <param name="testState">새로운 Test 상태</param>
    /// <returns>갱신 수행 성공 여부</returns>
    public async Task<ITestItem> UpdateTestStateAsync(int testItemId, TestStates testState)
    {
        try
        {
            return await _writer.EnqueueWriteAsync(async context =>
            {
                var testItem = await context.TestItems.AsNoTracking()
                                                      .Include(TI => TI.TestOption)
                                                      .Include(TI => TI.Sample)
                                                      .Include(TI => TI.TipCartridge)
                                                      .Include(TI => TI.Panel)
                                                         .ThenInclude(P => P.dKit)
                                                             .ThenInclude(K => K.DrugTestInfomations)
                                                      .Include(TI => TI.Panel)
                                                         .ThenInclude(P => P.dKit)
                                                             .ThenInclude(K => K.DispensingPositionOrders)
                                                      .Where(testItem => testItem.Id == testItemId)
                                                      .SingleAsync()
                    ?? throw new InvalidOperationException($"Cannot found a test item. TestItemId=[{testItemId}]");

                testItem.TestState = testState;
                testItem.UpdateAt  = DateTime.Now;

                await context.SaveChangesAsync();

                return testItem;
            });
        }
        finally
        {
        }
    }

    /// <summary>
    /// Test를 삭제
    /// </summary>
    /// <param name="testItemId">test id</param>
    public async Task DeleteTestItemAsync(int testItemId)
    {
        try
        {
            using var _DbContext = _contextFactory();
            using var DbTransaction = await _DbContext.Database.BeginTransactionAsync();

            var testItem = await _DbContext.TestItems.FirstOrDefaultAsync(item => item.Id == testItemId);

            if (testItem != null)
            {
                var testOption = await _DbContext.TestOptions.FirstOrDefaultAsync(option => option.Id == testItem.TestOptionId);

                if (testOption != null)
                {
                    _DbContext.TestOptions.Remove(testOption);

                    await _DbContext.SaveChangesAsync();
                }

                var testSequenceList = await _DbContext.TestSequences.Where(sequence => sequence.TestItemId == testItemId).ToListAsync();

                if (testSequenceList.Count > 0)
                {
                    _DbContext.TestSequences.RemoveRange(testSequenceList);

                    await _DbContext.SaveChangesAsync();
                }

                _DbContext.TestItems.Remove(testItem);

                await _DbContext.SaveChangesAsync();

                await DbTransaction.CommitAsync();
            }
        }
        finally
        {
        }
    }

    public async Task<IReadOnlyList<ITestSequence>> GetTestSequenceAsync()
    {
        try
        {
            using var _DbContext = _contextFactory();
            var testSequenceList = await _DbContext.TestSequences.AsNoTracking().ToListAsync();

            return testSequenceList;
        }
        catch (Exception ex)
        {
            if (ex.Message != null &&
                ex.Message?.Contains("A second operation was started on this context instance before a previous operation completed") == true)
            {
                var message = $"DbContext Exception GetTestSequenceAsync  ThreadId: {Environment.CurrentManagedThreadId}";
                EventAggregator?.GetEvent<DbServiceExceptionEvent>().Publish(new DbServiceExceptionEventArgs(message, ex));
            }
            return null;
        }
    }

    /// <summary>
    /// Id에 해당 되는 TestSeqeunce 획득
    /// </summary>
    /// <param name="testItemId">test id</param>
    /// <returns>해당 TestSequence 정보</returns>
    public async Task<IReadOnlyList<ITestSequence>> GetTestSequenceByTestIdAsync(int testItemId)
    {
        using var _DbContext = _contextFactory();
        var testSequenceList = await _DbContext.TestSequences.AsNoTracking()
                                                                 .Where(sequence => sequence.TestItemId == testItemId)
                                                                 .ToListAsync();

        return testSequenceList;
    }

    public async Task<ITestSequence> GetTestSequenceAsync(string name)
    {
        try
        {
            using var _DbContext = _contextFactory();
            var item = await _DbContext.TestSequences.AsNoTracking()
                                                         .Where(sequence => sequence.Name == name)
                                                         .FirstAsync();
            return item;
        }
        catch (Exception ex)
        {
            if (ex.Message != null &&
                ex.Message?.Contains("A second operation was started on this context instance before a previous operation completed") == true)
            {
                var message = $"DbContext Exception GetTestSequenceAsync(Name)  ThreadId: {Environment.CurrentManagedThreadId}";
                EventAggregator?.GetEvent<DbServiceExceptionEvent>().Publish(new DbServiceExceptionEventArgs(message, ex));
            }
            return null;
        }
        finally
        {
        }
    }

    /// <summary>
    /// TestSequence를 추가한다.
    /// </summary>
    /// <param name="testItemId"><see cref="ITestItem.Id"/></param>
    /// <param name="name">Activity 명</param>
    /// <param name="sequenceState">Activity 상태</param>
    public async Task AddTestSequenceAsync(int testItemId, string name, SequenceStates sequenceState)
    {
        try
        {
            var testSequence = new TestSequence { TestItemId = testItemId, Name = name, SequenceState = sequenceState, StartAt = DateTime.Now };

            await _writer.EnqueueWriteAsync(async context =>
            {
                Logger.WriteLog(LogTypes.Debug, $"Add testSequence : TestItemId {testItemId},Name=[{name}], SequenceStates: {sequenceState}");
                await context.AddTestSequenceAsync(testSequence);
            });

            //await RetryHelper.ExecuteWithRetry(async () =>
            //{
            //    using var _DbContext = _contextFactory();
            //    Logger.WriteLog(LogTypes.Debug, $"Add testSequence : TestItemId {testItemId},Name=[{name}], SequenceStates: {sequenceState}");
            //    await _DbContext.AddTestSequenceAsync(testSequence);
            //}, maxRetries: 3);
        }
        catch (Exception ex)
        {
            if (ex.Message != null &&
                ex.Message?.Contains("A second operation was started on this context instance before a previous operation completed") == true)
            {
                var message = $"DbContext Exception AddTestSequenceAsync  ThreadId: {Environment.CurrentManagedThreadId}";
                EventAggregator?.GetEvent<DbServiceExceptionEvent>().Publish(new DbServiceExceptionEventArgs(message, ex));
            }
        }
        finally
        {
        }
    }

    /// <summary>
    /// TestSequence의 상태를 업데이트 한다.
    /// </summary>
    /// <param name="testItemId"><see cref="ITestItem.Id"/></param>
    /// <param name="name">Activity 명</param>
    /// <param name="sequenceState">Activity 상태</param>
    public async Task<int> UpdateTestSequenceStateAsync(int testItemId, string name, SequenceStates sequenceState)
    {
        Logger.WriteLog(LogTypes.Debug, $"Entered SemaphoreSlim at {DateTime.UtcNow},TestItemId=[{testItemId}], ThreadId: {Environment.CurrentManagedThreadId}");
        FormattableString sql;
        try
        {
            int rowsModified = 0;
            Logger.WriteLog(LogTypes.Debug, $"Calling ExecuteUpdateAsync at {DateTime.UtcNow},TestItemId=[{testItemId}], ThreadId: {Environment.CurrentManagedThreadId}");
            
            sql = FormattableStringFactory.Create($"UPDATE TestSequence " +
                                                "SET SequenceState = {0}, EndAt = {1} " +
                                                "WHERE TestItemId = {2} AND Name = {3};",
                                                (int)sequenceState, DateTime.Now, testItemId, name);

            rowsModified = await _writer.EnqueueWriteAsync(async context =>
            {
                Logger.WriteLog(LogTypes.Debug, $"sql = {sql.ToString()}");
                Logger.WriteLog(LogTypes.Debug, $"ExecuteUpdateAsync completed at {DateTime.UtcNow},TestItemId=[{testItemId}], RowsModified: {rowsModified}, ThreadId: {Environment.CurrentManagedThreadId}");
                return await context.Database.ExecuteSqlAsync(sql);
            });

            if (rowsModified != 1)
            {
                Logger.WriteLog(LogTypes.Debug, $"Failed to update test sequence state. TestItemId=[{testItemId}], Name=[{name}], SequenceState=[{sequenceState}] :: rowsModified[{rowsModified}]");
            }
            return rowsModified;
        }
        catch (Exception ex)
        {
            if (ex.Message != null &&
                ex.Message?.Contains("A second operation was started on this context instance before a previous operation completed") == true)
            {
                var message = $"DbContext Exception UpdateTestSequenceStateAsync  ThreadId: {Environment.CurrentManagedThreadId}";
                EventAggregator?.GetEvent<DbServiceExceptionEvent>().Publish(new DbServiceExceptionEventArgs(message, ex));
            }
            return -1;
        }
        finally
        {
            Logger.WriteLog(LogTypes.Debug, $"Entered SemaphoreSlim at {DateTime.UtcNow},TestItemId=[{testItemId}], ThreadId: {Environment.CurrentManagedThreadId}");
        }
    }

    #endregion // Methods
}

public static class DbContextExtensions
{
    #region TestSequence

    public static void AddTestSequence(this DbContext DbContext, ITestSequence testSequence)
    {
        var rowsModified = DbContext.Database.ExecuteSql(TestSequence.MakeInsertQuery(testSequence));

        if (rowsModified != 1)
        {
            throw new InvalidOperationException($"Failed to insert test sequence. TestItemId=[{testSequence.TestItemId}], Name=[{testSequence.Name}]");
        }
    }

    public static async Task AddTestSequenceAsync(this DbContext DbContext, ITestSequence testSequence)
    {
        var rowsModified = await DbContext.Database.ExecuteSqlAsync(TestSequence.MakeInsertQuery(testSequence));

        if (rowsModified != 1)
        {
            throw new InvalidOperationException($"Failed to insert test sequence. TestItemId=[{testSequence.TestItemId}], Name=[{testSequence.Name}]");
        }
    }

    public static void RemoveTestSequence(this DbContext DbContext, int testItemId)
    {
        var rowsModified = DbContext.Database.ExecuteSql(TestSequence.MakeDeleteQuery(testItemId));

        if (rowsModified < 0)
        {
            throw new InvalidOperationException($"Failed to delete panel locations. TestItemId=[{testItemId}]");
        }
    }

    public static async Task RemoveTestSequenceAsync(this DbContext DbContext, int testItemId)
    {
        var rowsModified = await DbContext.Database.ExecuteSqlAsync(TestSequence.MakeDeleteQuery(testItemId));

        if (rowsModified < 0)
        {
            throw new InvalidOperationException($"Failed to delete panel locations. TestItemId=[{testItemId}]");
        }
    }

    #endregion
}

public class DbServiceExceptionEventArgs : EventArgs
{
    public string Message { get; }
    public Exception Exception { get; }
    public DbServiceExceptionEventArgs(string message, Exception exception)
    {
        Message   = message;
        Exception = exception;
    }
}
public class DbServiceExceptionEvent : PubSubEvent<DbServiceExceptionEventArgs>
{
}
