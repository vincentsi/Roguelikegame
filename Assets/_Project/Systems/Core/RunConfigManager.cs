namespace ProjectRoguelike.Core
{
    /// <summary>
    /// Manages the current run configuration. Acts as a service to pass run config between states.
    /// </summary>
    public sealed class RunConfigManager
    {
        private RunConfig _currentRunConfig;

        public RunConfig CurrentRunConfig => _currentRunConfig;

        public void SetRunConfig(RunConfig config)
        {
            _currentRunConfig = config;
        }

        public void ClearRunConfig()
        {
            _currentRunConfig = null;
        }
    }
}

