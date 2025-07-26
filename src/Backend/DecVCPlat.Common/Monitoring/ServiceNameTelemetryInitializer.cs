using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace DecVCPlat.Common.Monitoring
{
    /// <summary>
    /// Telemetry initializer that adds the service name to all telemetry items
    /// </summary>
    public class ServiceNameTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _serviceName;

        public ServiceNameTelemetryInitializer(string serviceName)
        {
            _serviceName = serviceName;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (!string.IsNullOrEmpty(_serviceName))
            {
                telemetry.Context.Cloud.RoleName = _serviceName;
                
                // Add service name as a custom property for easier filtering
                telemetry.Context.GlobalProperties["ServiceName"] = _serviceName;
            }
        }
    }
}
