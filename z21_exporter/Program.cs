using CommandLine.Text;
using CommandLine;
using Prometheus;
using System.Net;
using Z21.DTO;
using Microsoft.Extensions.Logging;
using Z21;

namespace z21_exporte
{
    public class Options
    {
        [Option('t', "target", Required = false, HelpText = "Ip Adress of the z21 target")]
        public string IpAddress { get; set; } = "192.168.0.111";

        [Option('p', "port", Required = false, HelpText = "Port of this exporter")]
        public int Port { get; set; } = 9101;

        [Option('r', "rate", Required = false, HelpText = "How often (in seconds) the target should be pulled for data.")]
        public int Rate { get; set; } = 15;
    }

    public class Program
    {
        private static readonly Gauge MainCurrent = Metrics.CreateGauge("z21Exporter_systemstate_maincurrent", "Strom am Hauptgleis in Ampere (A)", new GaugeConfiguration { SuppressInitialValue = true });
        private static readonly Gauge ProgCurrent = Metrics.CreateGauge("z21Exporter_systemstate_progcurrent", "Strom am Programmiergleis in Ampere (A)", new GaugeConfiguration { SuppressInitialValue = true });
        private static readonly Gauge FilteredMainCurrent = Metrics.CreateGauge("z21Exporter_systemstate_filteredmaincurrent", "geglätteter Strom am Hauptgleis in Ampere (A)", new GaugeConfiguration { SuppressInitialValue = true });
        private static readonly Gauge Temperature = Metrics.CreateGauge("z21Exporter_systemstate_temperature", "interne Temperatur in der Zentrale in Celcius (C)", new GaugeConfiguration { SuppressInitialValue = true });
        private static readonly Gauge SupplyVoltage = Metrics.CreateGauge("z21Exporter_systemstate_supplyvoltage", "Versorgungsspannung in Volt (V)", new GaugeConfiguration { SuppressInitialValue = true });
        private static readonly Gauge VCCVoltage = Metrics.CreateGauge("z21Exporter_systemstate_vccvoltage", "interne Spannung, identisch mit Gleisspannung in Volt (V)", new GaugeConfiguration { SuppressInitialValue = true });

        private static System.Timers.Timer Timer { get; } = new System.Timers.Timer(new TimeSpan(0, 0, 15).TotalMilliseconds) { AutoReset = true, Enabled = true };

        public static Z21.Client Client { get; } = new Z21.Client();

        public static void Main(string[] args) => Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
        {
            try
            {
                Client.Connect(IPAddress.Parse(o.IpAddress));
                Client.OnSystemStateDataChanged += (a, b) => SetValues(b.Data);
                Client.ClientReachabilityChanged += Client_ClientReachabilityChanged;
                Client.LogMessage += (a, b) => Log($"Z21 client: {b.Message}");
                Log("Z21 initialized.");

                Timer.Interval = new TimeSpan(0, 0, o.Rate).TotalMilliseconds;
                Timer.Elapsed += (a, b) => Client.SystemStateGetData();

                var metricServer = new KestrelMetricServer(port: o.Port);
                metricServer.Start();
                Log($"Server started on port {o.Port}.");

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Log($"{ex}");
            }
        });

        private static void Client_ClientReachabilityChanged(object? sender, bool e)
        {
            if (!e)
            {
                MainCurrent.Unpublish();
                ProgCurrent.Unpublish();
                FilteredMainCurrent.Unpublish();
                Temperature.Unpublish();
                SupplyVoltage.Unpublish();
                VCCVoltage.Unpublish();
            }
        }

        private static void SetValues(SystemStateData e)
        {
            if (e is null || e?.ClientData is null)
                return;
            MainCurrent.Set((double)(e.MainCurrent / 1000.0m));
            ProgCurrent.Set((double)(e.ProgCurrent / 1000.0m));
            FilteredMainCurrent.Set((double)(e.FilteredMainCurrent / 1000.0m));
            Temperature.Set(e.Temperature);
            SupplyVoltage.Set((double)(e.SupplyVoltage / 1000.0m));
            VCCVoltage.Set((double)(e.VCCVoltage / 1000.0m));
        }

        private static void Log(string txt) => Console.WriteLine(txt);
    }
}