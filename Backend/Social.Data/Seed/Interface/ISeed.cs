namespace Social.Data.Seed.Interface
{
    public interface ISeed<T>
    {
        T[] GetSeedData { get; }
    }
}
