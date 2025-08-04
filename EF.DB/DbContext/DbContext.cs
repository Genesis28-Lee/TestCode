using Microsoft.EntityFrameworkCore;

using System.IO;

using static EF.Data.DBContexts.CommonValues;

namespace EF.Data.DBContexts;


public class CommonValues
{
    public const int SlotNumberConvertBase = 9;
    public const int PanelSlotNumberMax = 4;
    public const int SampleSlotNumberMax = 4;
    public const int IncubatorSlotNumberMax = 8;
}

public abstract class SqliteDbContextBase : DbContext
{
    /// <summary>
    /// 데이터베이스 파일 정보를 가져오거나 설정한다.
    /// </summary>
    public FileInfo DatabaseFileInfo { get; protected set; }

    /// <summary>
    /// 해당 파일경로로 데이터베이스를 초기화한다.
    /// </summary>
    /// <param name="databaseFilePath">데이터베이스 경로</param>
    protected SqliteDbContextBase(string databaseFilePath)
    {
        try
        {
            DatabaseFileInfo = new FileInfo(databaseFilePath);

            if (string.IsNullOrWhiteSpace(DatabaseFileInfo.Name))
            {
                throw new ArgumentException("The database file name is null or empty.", nameof(databaseFilePath));
            }
        }
        catch (Exception e)
        {
            throw new ArgumentException("Invalid database file path.", e);
        }
    }
    /// <summary>
    /// <see cref="DbContextOptions"/>객체를 이용하여 데이터베이스를 초기화한다.
    /// </summary>
    /// <param name="options"></param>
    protected SqliteDbContextBase(DbContextOptions options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (DatabaseFileInfo != null)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabaseFileInfo.FullName};Pooling=False")
                          .EnableSensitiveDataLogging(true);
        }
        else
        {
            base.OnConfiguring(optionsBuilder);
        }
    }

    private string GetProductVersion()
    {
        var assemblyName = typeof(DbContext).Assembly.GetName();
        var version = assemblyName.Version.ToString(3);

        return version;
    }
}

public class DbContext : SqliteDbContextBase
{
    public int NumberOfIncubator  { get; private set; }
    public int NumberOfSampleSlot { get; private set; }
    public int NumberOfPanelSlot  { get; private set; }


    #region DBSet
    public DbSet<TestOption>    TestOptions     { get; set; }
    public DbSet<TestItem>      TestItems       { get; set; }
    public DbSet<TestSequence>  TestSequences   { get; set; }
    public DbSet<IncubatorSlot> IncubatorSlots  { get; set; }
    #endregion //DBSet


    public DbContext(string databaseFilePath, int numberOfIncubator = IncubatorSlotNumberMax, int numberOfSampleSlot = SampleSlotNumberMax, int numberOfPanelSlot = PanelSlotNumberMax)
        : base(databaseFilePath)
    {
        Initialize(numberOfIncubator, numberOfSampleSlot, numberOfPanelSlot);
    }

    public DbContext(DbContextOptions<DbContext> options, int numberOfIncubator = IncubatorSlotNumberMax, int numberOfSampleSlot = SampleSlotNumberMax, int numberOfPanelSlot = PanelSlotNumberMax)
        : base(options)
    {
        Initialize(numberOfIncubator, numberOfSampleSlot, numberOfPanelSlot);
    }


    private void Initialize(int numberOfIncubator, int numberOfSampleSlot, int numberOfPanelSlot)
    {
        if (numberOfIncubator <= 0)
        {
            throw new ArgumentOutOfRangeException("The number of incubator slot must be 1 or greater.", nameof(numberOfIncubator));
        }

        if (numberOfSampleSlot <= 0)
        {
            throw new ArgumentOutOfRangeException("The number of sample slot must be 1 or greater.", nameof(numberOfSampleSlot));
        }

        if (numberOfPanelSlot <= 0)
        {
            throw new ArgumentOutOfRangeException("The number of panel slot must be 1 or greater.", nameof(numberOfPanelSlot));
        }

        NumberOfIncubator  = numberOfIncubator;
        NumberOfSampleSlot = numberOfSampleSlot;
        NumberOfPanelSlot  = numberOfPanelSlot;

        // SQLite 데이터베이스에서 WAL 모드를 활성화하여 동시성 문제를 줄이고 성능을 향상시킬 수 있습니다.
        Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 모델 생성 로직을 여기에 추가합니다.
        // 예: modelBuilder.Entity<YourEntity>().ToTable("YourTableName");
        // 예시로 Incubator, SampleSlot, PanelSlot 엔티티를 추가할 수 있습니다.
        // modelBuilder.Entity<Incubator>().ToTable("Incubators");
        // modelBuilder.Entity<SampleSlot>().ToTable("SampleSlots");
        // modelBuilder.Entity<PanelSlot>().ToTable("PanelSlots");

        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
    }
}
