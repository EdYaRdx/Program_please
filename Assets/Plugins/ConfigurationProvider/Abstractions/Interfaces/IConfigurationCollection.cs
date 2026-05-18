using System;

namespace ConfigurationProvider
{
    public interface IConfigurationCollection
    {
        bool TryGet<T>(out T configuration) where T : class;
        bool TryGet(Type type, out object configuration);
    }
}
