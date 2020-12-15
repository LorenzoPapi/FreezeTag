using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Impostor.Api.Innersloth;
using Impostor.Api.Innersloth.Customization;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace FreezeTag
{
    public class FreezeTagEventListener : IEventListener
    {
        private readonly List<IGame> DeactivatedGames = new List<IGame>();
        private readonly Dictionary<IGame, FreezeTagInfos> CodeAndInfos = new Dictionary<IGame, FreezeTagInfos>();
        private readonly ILogger<FreezeTagPlugin> _logger;

        public FreezeTagEventListener(ILogger<FreezeTagPlugin> logger)
        {
            _logger = logger;
        }

        [EventListener]
        public async ValueTask OnGameStarting(IGameStartingEvent e)
        {
            if (!DeactivatedGames.Contains(e.Game))
            {
                e.Game.Options.KillCooldown = int.MaxValue;
                e.Game.Options.NumEmergencyMeetings = 0;
                await e.Game.SyncSettingsAsync();
            }

        }

        [EventListener]
        public async ValueTask OnGameStarted(IGameStartedEvent e)
        {
            if (!DeactivatedGames.Contains(e.Game))
            {
                List<IClientPlayer> impostors = new List<IClientPlayer>();
                ConcurrentDictionary<IClientPlayer, Vector2> frozen = new ConcurrentDictionary<IClientPlayer, Vector2>();

                foreach (var player in e.Game.Players)
                {
                    if (player.Character.PlayerInfo.IsImpostor)
                    {
                        await player.Character.SetColorAsync(ColorType.Red);
                        impostors.Add(player);
                    }
                    else
                    {
                        await player.Character.SetColorAsync(ColorType.Green);
                    }
                }
                CodeAndInfos.Add(e.Game, new FreezeTagInfos(impostors, frozen));
            }
        }

        [EventListener]
        public async ValueTask OnPlayerMovement(IPlayerMovementEvent e)
        {
            if (CodeAndInfos.ContainsKey(e.Game))
            {
                List<IClientPlayer> impostors = CodeAndInfos[e.Game].impostors;
                ConcurrentDictionary<IClientPlayer, Vector2> frozens = CodeAndInfos[e.Game].frozens;
                IEnumerable<IClientPlayer> crewmates = e.Game.Players.Except(impostors).Except(frozens.Keys);

                if (!crewmates.Any())
                {
                    foreach (var nonImpostor in e.Game.Players.Except(impostors))
                    {
                        await nonImpostor.KickAsync();
                    }
                }

                foreach (var impostor in impostors)
                {
                    if (impostor.Character != null)
                    {
                        foreach (var crewmate in crewmates)
                        {
                            if (CheckIfColliding(crewmate, impostor))
                            {
                                frozens.TryAdd(crewmate, crewmate.Character.NetworkTransform.Position);
                                await crewmate.Character.SetColorAsync(ColorType.Blue);
                            }
                        }
                    }
                }

                foreach (var pair in frozens)
                {
                    IClientPlayer frozen = pair.Key;
                    Vector2 position = pair.Value;
                    if (frozen.Character.NetworkTransform.Position != position)
                    {
                        await frozen.Character.NetworkTransform.SnapToAsync(position);
                    }
                    foreach (var sun in crewmates)
                    {
                        if (sun != frozen && CheckIfColliding(sun, frozen))
                        {
                            await Unfreeze(frozen).ConfigureAwait(true);
                            frozens.Remove(frozen, out position);
                        }
                    }
                }
            }
        }

        private bool CheckIfColliding(IClientPlayer player1, IClientPlayer player2)
        {
            Vector2 crewmatePos = player1.Character.NetworkTransform.Position;
            Vector2 impostorPos = player2.Character.NetworkTransform.Position;
            float crewmateX = (float)Math.Round(crewmatePos.X, 1);
            float crewmateY = (float)Math.Round(crewmatePos.Y, 1);
            float impostorX = (float)Math.Round(impostorPos.X, 1);
            float impostorY = (float)Math.Round(impostorPos.Y, 1);
            if (crewmateX <= impostorX + 0.2 && crewmateX >= impostorX - 0.2 && crewmateY <= impostorY + 0.2 && crewmateY >= impostorY - 0.2)
            {
                return true;
            }
            return false;
        }

        private async ValueTask Unfreeze(IClientPlayer frozen)
        {
            Thread.Sleep(2500);
            await frozen.Character.SetColorAsync(ColorType.Green);
        }

        [EventListener]
        public void OnGameEnded(IGameEndedEvent e)
        {
            if (CodeAndInfos.ContainsKey(e.Game))
            {
                CodeAndInfos[e.Game].frozens.Clear();
                CodeAndInfos[e.Game].impostors.Clear();
                CodeAndInfos.Remove(e.Game);
            }
        }

        [EventListener]
        public async ValueTask OnPlayerDeath(IPlayerMurderEvent e)
        {
            if (CodeAndInfos.ContainsKey(e.Game))
            {
                foreach (var impostor in CodeAndInfos[e.Game].impostors)
                {
                    await impostor.KickAsync();
                }
            }
        }

        [EventListener]
        public async ValueTask OnPlayerChat(IPlayerChatEvent e)
        {
            if (e.Game.GameState == GameStates.NotStarted && e.Message.StartsWith("/ftag "))
            {
                switch (e.Message.ToLowerInvariant()[6..])
                {
                    case "on":
                        if (e.ClientPlayer.IsHost)
                        {
                            if (DeactivatedGames.Contains(e.Game))
                            {
                                DeactivatedGames.Remove(e.Game);
                                await ServerSendChatAsync("Freeze Tag activated for this game.", e.ClientPlayer.Character);
                            } else
                            {
                                await ServerSendChatAsync("Freeze Tag was already active.", e.ClientPlayer.Character);
                            }
                        } else
                        {
                            await ServerSendChatToPlayerAsync("You can't enable Freeze Tag because you aren't the host.", e.ClientPlayer.Character);
                        }
                        break;
                    case "off":
                        if (e.ClientPlayer.IsHost)
                        {
                            if (!DeactivatedGames.Contains(e.Game))
                            {
                                DeactivatedGames.Add(e.Game);
                                await ServerSendChatAsync("Freeze Tag deactivated for this game.", e.ClientPlayer.Character);
                            }
                            else
                            {
                                await ServerSendChatAsync("Freeze Tag was already off.", e.ClientPlayer.Character);
                            }
                        }
                        else
                        {
                            await ServerSendChatToPlayerAsync("You can't disable Freeze Tag because you aren't the host.", e.ClientPlayer.Character);
                        }
                        break;
                    case "help":
                        await ServerSendChatToPlayerAsync("Freeze Tag is a custom Among Us mode.", e.ClientPlayer.Character);
                        await ServerSendChatToPlayerAsync("Impostors are red, crewmates are green. The impostors can freeze the crewmates by standing near them.", e.ClientPlayer.Character);
                        await ServerSendChatToPlayerAsync("The crewmates can unfreeze the frozen crewmates by standing near of them.", e.ClientPlayer.Character);
                        await ServerSendChatToPlayerAsync("Objective of the impostors: freeze everyone.", e.ClientPlayer.Character);
                        await ServerSendChatToPlayerAsync("Objective of the crewmates: finish all their tasks.", e.ClientPlayer.Character);
                        await ServerSendChatToPlayerAsync("If a crewmate gets killed, the impostors get killed!", e.ClientPlayer.Character);
                        break;
                }
            }
        }

        private async ValueTask ServerSendChatAsync(string text, IInnerPlayerControl player)
        {
            string playername = player.PlayerInfo.PlayerName;
            await player.SetNameAsync($"PublicMsg");
            await player.SendChatAsync($"{text}");
            await player.SetNameAsync(playername);
        }

        private async ValueTask ServerSendChatToPlayerAsync(string text, IInnerPlayerControl player)
        {
            string playername = player.PlayerInfo.PlayerName;
            await player.SetNameAsync($"PrivateMsg");
            await player.SendChatToPlayerAsync($"{text}");
            await player.SetNameAsync(playername);
        }
    }
}