namespace OptInStorage;

public class OptInRepository
{
    private static readonly object LockObject = new ();
    private readonly string fileName;

    public OptInRepository(string fileName)
    {
        this.fileName = fileName;
    }

    public void OptIn(string key)
    {
        lock (LockObject)
        {
            File.AppendAllLines(fileName, new[] {key});
        }
    }

    public IEnumerable<string> GetOptIns() => File.ReadAllLines(fileName).Distinct().ToArray();
}

public class OptInNumbersRepository : OptInRepository
{
    public OptInNumbersRepository(string fileName) : base(fileName)
    {
    }
    
}

public class OptInEmailsRepository : OptInRepository
{
    public OptInEmailsRepository(string fileName) : base(fileName)
    {
    }
}