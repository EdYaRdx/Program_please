namespace ConfigurationProvider
{
    public interface IConfigurationBuilder : IConfigurationCollection
    {
        void AddConfiguration(object configuration);
        void AddConfiguration<T>(T configuration);

        IConfigurationCollection Build();
    }
}
