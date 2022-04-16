using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using IOTLinkAPI.Addons;
using IOTLinkAPI.Helpers;
using IOTLinkAPI.Platform.Events;

namespace IncognitoAddon
{
    public class IncognitoAddonAgent : AgentAddon
    {
        public override void Init(IAddonManager addonManager)
        {
            base.Init(addonManager);
            OnAgentRequestHandler += OnAgentRequest;
        }

        private void OnAgentRequest(object sender, AgentAddonRequestEventArgs e)
        {
            LoggerHelper.Verbose("ProcessMonitorAgent::OnAgentRequest");

            AddonRequestType requestType = e.Data.requestType;

            switch (requestType)
            {
                case AddonRequestType.REQUEST_INCOGNITO_INFORMATION:
                    var isIncognito = IsIncognito();

                    SendResponse(isIncognito);
                    break;

                default: break;
            }
        }

        private void SendResponse(bool isIncognito)
        {
            dynamic addonData = new ExpandoObject();
            addonData.requestType = AddonRequestType.REQUEST_INCOGNITO_INFORMATION;
            addonData.requestData = isIncognito;
            GetManager().SendAgentResponse(this, addonData);
        }


        private bool IsIncognito() => Process.GetProcesses().Any(process => process.ProcessName == "chrome" && WmiTest(process.Id));

        private bool WmiTest(int processId)
        {
            ProcessInformation.Retrieve(processId, out var commandLine);

            return commandLine.Contains("--disable-databases");
        }
    }
}