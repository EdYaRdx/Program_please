using System;

namespace EntryPoint
{
    public class BuilderAlreadyBuildedException : Exception
    {
        public override string Message => _message;

        private string _message;

        public BuilderAlreadyBuildedException(string builderName) 
        {
            _message = $"Cannot use some methods after building of {builderName}";
        }
    }
}
