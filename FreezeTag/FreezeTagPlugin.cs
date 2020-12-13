using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FreezeTag
{
    [ImpostorPlugin(
        package: "LorenzoPapi\'s Gamemodes",
        name: "Freeze Tag",
        author: "LorenzoPapi",
        version: "1.0.0")]
    public class FreezeTagPlugin : PluginBase
    {
        private readonly ILogger<FreezeTagPlugin> _logger;

        private readonly IEventManager _eventManager;

        private IDisposable _unregister;

        public FreezeTagPlugin(ILogger<FreezeTagPlugin> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public override ValueTask EnableAsync()
        {
            _logger.LogInformation("Loaded Freeze Tag Gamemode");
            _unregister = _eventManager.RegisterListener(new FreezeTagEventListener(_logger));
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _logger.LogInformation("Unloaded Freeze Tag Gamemode");
            _unregister.Dispose();
            return default;
        }
    }
}