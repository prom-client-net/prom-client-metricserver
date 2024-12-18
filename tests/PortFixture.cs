using System.Net;
using System.Net.Sockets;

namespace Prometheus.Client.MetricServer.Tests;

public class PortFixture
{
    public int Port { get; } = FindAvailablePort();

    private static int FindAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);

        try
        {
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            return port;
        }
        finally
        {
            listener.Stop();
        }
    }
}
