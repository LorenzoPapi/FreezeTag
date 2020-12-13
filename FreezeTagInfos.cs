using Impostor.Api.Net;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace FreezeTag
{
    public class FreezeTagInfos
    {

        public readonly List<IClientPlayer> impostors;
        public readonly ConcurrentDictionary<IClientPlayer, Vector2> frozens;
        
        public FreezeTagInfos(List<IClientPlayer> impostors, ConcurrentDictionary<IClientPlayer, Vector2> frozens)
        {
            this.impostors = impostors;
            this.frozens = frozens;
        }

        
    }
}
