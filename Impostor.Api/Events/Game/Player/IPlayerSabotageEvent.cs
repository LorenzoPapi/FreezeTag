namespace Impostor.Api.Events.Player
{
    public interface IPlayerSabotageEvent : IPlayerEvent
    {
        public byte Amount { get; }
    }
}
