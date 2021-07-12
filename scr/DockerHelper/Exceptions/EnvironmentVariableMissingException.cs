using System;
using System.Runtime.Serialization;

namespace DockerHelper.Exceptions
{
    public class EnvironmentVariableMissingException : Exception
    {
        public EnvironmentVariableMissingException()
        {
        }

        public EnvironmentVariableMissingException(string message) : base(message)
        {
        }

        public EnvironmentVariableMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EnvironmentVariableMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
