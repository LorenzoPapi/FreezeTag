using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner.Objects;

namespace Impostor.Server.Events.Player
{
    public class PlayerSabotageEvent : IPlayerSabotageEvent
    {
        public PlayerSabotageEvent(IGame game, IClientPlayer clientPlayer, IInnerPlayerControl playerControl, byte amount)
        {
            Game = game;
            ClientPlayer = clientPlayer;
            PlayerControl = playerControl;
            Amount = amount;
        }

        public IGame Game { get; }

        public IClientPlayer ClientPlayer { get; }

        public IInnerPlayerControl PlayerControl { get; }

        public byte Amount { get; }
    }
}
