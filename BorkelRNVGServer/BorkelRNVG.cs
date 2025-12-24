using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;

namespace BorkelRNVGServer
{
    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
    public class BorkelRNVG(ISptLogger<BorkelRNVG> logger) : IOnLoad
    {
        public Task OnLoad()
        {
            logger.LogWithColor("Successfully loaded Borkel's realistic NVGs!", LogTextColor.Green);
            
            return Task.CompletedTask;   
        }
    }
}
