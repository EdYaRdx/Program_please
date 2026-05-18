using System;

namespace ConfigurationProvider
{
    public class MissingConfigurationException : Exception
    {
        public override string Message => _message;

        private string _message;

        public MissingConfigurationException(Type missingConfigurationType)
        {
            _message = $"Cannot find configuration of type {missingConfigurationType.Name}";
        }
    }
}
