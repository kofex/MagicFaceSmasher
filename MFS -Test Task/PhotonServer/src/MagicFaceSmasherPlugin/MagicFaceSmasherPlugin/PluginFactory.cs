using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Hive.Plugin;

namespace MagicFaceSmasherPlugin
{
    public class PluginFactory : IPluginFactory
    {
        public IGamePlugin Create(IPluginHost gameHost, string pluginName, Dictionary<string, string> config, out string errorMsg)
        {
            PluginBase plugin = null;

            if (pluginName.Equals(typeof(MagicFaceSmasherPlugin).Name))
            {
                plugin = new MagicFaceSmasherPlugin();
            }

            if (plugin.SetupInstance(gameHost, config, out errorMsg))
                return plugin;
            else
                return null;
        }
    }
}
