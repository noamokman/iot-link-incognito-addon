using System;
using System.Timers;
using System.IO;
using IOTLinkAPI.Addons;
using IOTLinkAPI.Configs;
using IOTLinkAPI.Helpers;
using IOTLinkAPI.Platform;
using IOTLinkAPI.Platform.Events;
using System.Dynamic;

namespace IncognitoAddon
{
    public class IncognitoAddonService : ServiceAddon
    {
        private string _configPath;
        private Configuration _config;
        private Timer _monitorTimer;
        private Cache _cache;

        public override void Init(IAddonManager addonManager)
        {
            base.Init(addonManager);

            LoggerHelper.Verbose("IncognitoAddonService::Init() - Started");

            var cfgManager = ConfigurationManager.GetInstance();
            _configPath = Path.Combine(_currentPath, "config.yaml");
            _config = cfgManager.GetConfiguration(_configPath);
            cfgManager.SetReloadHandler(_configPath, OnConfigReload);
            _cache = new Cache();

            OnConfigReloadHandler += OnConfigReload;
            OnMQTTConnectedHandler += OnMQTTConnected;
            OnMQTTDisconnectedHandler += OnMQTTDisconnected;
            OnRefreshRequestedHandler += OnClearEvent;
            OnAgentResponseHandler += OnAgentResponse;

            Restart();

            LoggerHelper.Verbose("IncognitoAddonService::Init() - Completed");
        }

        private bool IsAddonEnabled => _config != null && _config.GetValue("enabled", false);

        private void Restart()
        {
            _cache.Clear();
            CleanTimers();
            StartTimers();
        }


        private void CleanTimers()
        {
            _monitorTimer?.Stop();
            _monitorTimer = null;
        }

        private void StartTimers()
        {
            _monitorTimer = new Timer();
            _monitorTimer.Elapsed += new ElapsedEventHandler(OnMonitorTimerElapsed);
            _monitorTimer.Interval = _config.GetValue("interval", 5000);
            _monitorTimer.Start();
        }

        private void OnMonitorTimerElapsed(object source, ElapsedEventArgs e)
        {
            LoggerHelper.Verbose("IncognitoAddonService::OnMonitorTimerElapsed() - Started");

            RequestAgentData();

            LoggerHelper.Verbose("IncognitoAddonService::OnMonitorTimerElapsed() - Completed");
        }
        private void RequestAgentData()
        {
            dynamic addonData = new ExpandoObject();
            addonData.requestType = AddonRequestType.REQUEST_INCOGNITO_INFORMATION;

            GetManager().SendAgentRequest(this, addonData);
        }


        private void OnMQTTConnected(object sender, EventArgs e)
        {
            Restart();
        }

        private void OnMQTTDisconnected(object sender, EventArgs e)
        {
            CleanTimers();
        }

        private void OnClearEvent(object sender, EventArgs e)
        {
            LoggerHelper.Verbose("IncognitoAddonService::OnClearEvent() - Clearing cache and resending information.");
        }

        private void OnConfigReload(object sender, ConfigReloadEventArgs e)
        {
            if (e.ConfigType != ConfigType.CONFIGURATION_ADDON)
                return;

            LoggerHelper.Verbose("IncognitoAddonService::OnConfigReload() - Reloading configuration");

            _config = ConfigurationManager.GetInstance().GetConfiguration(_configPath);
            Restart();
        }


        private void OnAgentResponse(object sender, AgentAddonResponseEventArgs e)
        {
            AddonRequestType requestType = (AddonRequestType)e.Data.requestType;

            if (requestType != AddonRequestType.REQUEST_INCOGNITO_INFORMATION)
            {
                return;
            }

            bool isIncognito = e.Data.requestData;

            bool? cached = _cache.Get("isIncognito");

            if (cached.HasValue && cached.Equals(isIncognito))
            {
                return;
            }

            _cache.Store("isIncognito", isIncognito, TimeSpan.FromMilliseconds(_config.GetValue("cacheTTL", 30000)));

            GetManager().PublishMessage(this, _config.GetValue("topic", "incognito"), isIncognito.ToString());
        }
    }
}
