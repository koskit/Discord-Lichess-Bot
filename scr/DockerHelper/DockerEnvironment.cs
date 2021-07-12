using DockerHelper.Exceptions;
using System;

namespace DockerHelper
{
    public static class DockerEnvironment
    {
        public static string GetEnvironmentVariable(string envName)
        {
            string envVariable = Environment.GetEnvironmentVariable(envName);

            if (string.IsNullOrWhiteSpace(envVariable))
                throw new EnvironmentVariableMissingException(
                    $"Environment variable \"{envName}\" is missing and is mandatory.");

            return envVariable;
        }

        public static string GetEnvironmentVariableOrDefault(string envName)
        {
            return Environment.GetEnvironmentVariable(envName);
        }

        public static string GetEnvironmentVariableWithFallback(string envName, string fallback)
        {
            string envVariable = Environment.GetEnvironmentVariable(envName);

            if (string.IsNullOrWhiteSpace(envVariable))
                envVariable = fallback;

            return envVariable;
        }

        public static T GetEnvironmentVariable<T>(string envName)
        {
            string envVariable = Environment.GetEnvironmentVariable(envName);

            if (string.IsNullOrWhiteSpace(envVariable))
                throw new EnvironmentVariableMissingException(
                    $"Environment variable \"{envName}\" is missing and is mandatory.");

            try
            {
                return (T)Convert.ChangeType(envVariable, typeof(T));
            }
            catch (Exception ex)
            {
                throw new EnvironmentVariableInvalidType(
                    $"Value of variable \"{envName}={envVariable}\" could not be parsed to {typeof(T).FullName}", ex);
            }
        }

        public static T GetEnvironmentVariableOrDefault<T>(string envName)
        {
            string envVariable = Environment.GetEnvironmentVariable(envName);

            if (string.IsNullOrWhiteSpace(envVariable))
                throw new EnvironmentVariableMissingException(
                    $"Environment variable \"{envName}\" is missing and is mandatory.");

            try
            {
                return (T)Convert.ChangeType(envVariable, typeof(T));
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static T GetEnvironmentVariableWithFallback<T>(string envName, T fallback)
        {
            string envVariable = Environment.GetEnvironmentVariable(envName);

            if (string.IsNullOrWhiteSpace(envVariable))
                throw new EnvironmentVariableMissingException(
                    $"Environment variable \"{envName}\" is missing and is mandatory.");

            try
            {
                return (T)Convert.ChangeType(envVariable, typeof(T));
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}
