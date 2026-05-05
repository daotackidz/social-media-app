namespace Social.Data
{
    public interface IDbInitializer
    {
        void Initialize();

        void SeedData();
    }
}
