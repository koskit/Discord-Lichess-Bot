using System;
using System.Runtime.Serialization;

namespace DockerHelper.Exceptions
{
    public class EnvironmentVariableInvalidType : Exception
    {
        public EnvironmentVariableInvalidType()
        {
        }

        public EnvironmentVariableInvalidType(string message) : base(message)
        {
        }

        public EnvironmentVariableInvalidType(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EnvironmentVariableInvalidType(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
