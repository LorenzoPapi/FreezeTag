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
        private readonly string help = "Freeze Tag is a custom Among Us mode. " +
                            "Impostors are red, crewmates are green. The impostors can freeze the crewmates by standing near them. " +
                            "The crewmates can unfreeze the frozen crewmates by standing near of them. " +
                            "Objective of the impostors: freeze everyone. " +
                            "Objective of the crewmates: finish all their tasks. " +
                            "If a crewmate gets killed or if an impostor sabotages, the impostors get killed." +
                            "If you're the host you can enable or disable the FreezeTag mode for the lobby with /ftag off or /ftag on" +
                            "Made by LorenzoPapi";
        private readonly List<IGame> DeactivatedGames = new();
        private readonly Dictionary<IGame, FreezeTagInfos> CodeAndInfos = new();
        private readonly ILogger<FreezeTagPlugin> _logger;
        private const float radius = 0.2f;

        public FreezeTagEventListener(ILogger<FreezeTagPlugin> logger)
        {
            _logger = logger;
        }

        [EventListener]
        public async ValueTask OnGameStarting(IGameStartingEvent e)
        {
            if (!DeactivatedGames.Contains(e.Game))
            {
                e.Game.Options.NumEmergencyMeetings = 0;
                await e.Game.SyncSettingsAsync();
            }

        }

        [EventListener]
        public async ValueTask OnGameStarted(IGameStartedEvent e)
        {
            if (!DeactivatedGames.Contains(e.Game))
            {
                List<IClientPlayer> impostors = new();
                ConcurrentDictionary<IClientPlayer, Vector2> frozen = new();

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
        public async ValueTask OnDoorStateChanged(IGameDoorStateChangedEvent e)
        {
            if (!e.IsOpen && CodeAndInfos.ContainsKey(e.Game))
            {
                foreach (var impostor in CodeAndInfos[e.Game].impostors.Where((player) => player != null && !player.Character.PlayerInfo.IsDead))
                {
                    await impostor.Character.SetMurderedByAsyncNoEvent(impostor);
                }
            }
            ClearDict(e);
        }

        [EventListener]
        public async ValueTask OnPlayerMovement(IPlayerMovementEvent e)
        {
            if (CodeAndInfos.ContainsKey(e.Game))
            {
                List<IClientPlayer> impostors = CodeAndInfos[e.Game].impostors;
                ConcurrentDictionary<IClientPlayer, Vector2> frozens = CodeAndInfos[e.Game].frozens;

                //Gives an iterator of all non-frozen crewmates
                IEnumerable<IClientPlayer> crewmates = e.Game.Players.Except(impostors).Except(frozens.Keys);

                //All crewmates are frozen, starting impostor winning process
                if (!crewmates.Any())
                {
                    //Every non impostor gets killed
                    foreach (var nonImpostor in e.Game.Players.Except(impostors))
                    {
                        await nonImpostor.Character.SetMurderedByAsyncNoEvent(impostors[0]);
                    }
                    ClearDict(e);
                }

                
                foreach (var impostor in impostors)
                {
                    //I am not updating the list if an impostor leaves, so I'll leave this check here for now
                    if (impostor.Character != null)
                    {
                        foreach (var crewmate in crewmates)
                        {   
                            //Checks if near impostor
                            if (CheckIfColliding(crewmate, impostor))
                            {
                                //Puts crewmate in list and makes him blue
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

                    //The frozen tries to move
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
            return (crewmateX <= impostorX + radius && crewmateX >= impostorX - radius && crewmateY <= impostorY + radius && crewmateY >= impostorY - radius);
        }

        private async ValueTask Unfreeze(IClientPlayer frozen)
        {
            Thread.Sleep(2500);
            await frozen.Character.SetColorAsync(ColorType.Green);
        }

        [EventListener]
        public void OnGameEnded(IGameEndedEvent e)
        {
            ClearDict(e);
        }

        private void ClearDict(IGameEvent e)
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
                foreach (var impostor in CodeAndInfos[e.Game].impostors.Where((player) => player != null && !player.Character.PlayerInfo.IsDead))
                {
                    await impostor.Character.SetMurderedByAsyncNoEvent(impostor);
                }
            }
            ClearDict(e);
        }

        [EventListener]
        public async ValueTask OnSabotage(IPlayerSabotageEvent e)
        {
            if (CodeAndInfos.ContainsKey(e.Game))
            {
                foreach (var impostor in CodeAndInfos[e.Game].impostors.Where((player) => player != null && !player.Character.PlayerInfo.IsDead))
                {
                    await impostor.Character.SetMurderedByAsyncNoEvent(impostor);
                }
            }
            ClearDict(e);
        }

        [EventListener]
        public async ValueTask OnPlayerChat(IPlayerChatEvent e)
        {
            await e.PlayerControl.SendChatAsync(e.Message);
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
                                await ServerSendChatAsync("Freeze Tag was already active.", e.ClientPlayer.Character, true);
                            }
                        } else
                        {
                            await ServerSendChatAsync("You can't enable Freeze Tag because you aren't the host.", e.ClientPlayer.Character, true);
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
                                await ServerSendChatAsync("Freeze Tag was already off.", e.ClientPlayer.Character, true);
                            }
                        }
                        else
                        {
                            await ServerSendChatAsync("You can't disable Freeze Tag because you aren't the host.", e.ClientPlayer.Character, true);
                        }
                        break;
                    case "help":
                        await ServerSendChatAsync(help, e.ClientPlayer.Character, true);
                        break;
                }
            }
        }

        private async ValueTask ServerSendChatAsync(string text, IInnerPlayerControl player, bool toPlayer = false)
        {
            string playername = player.PlayerInfo.PlayerName;
            byte playercolor = player.PlayerInfo.ColorId;
            await player.SetNameAsync("Server");
            await player.SetColorAsync(ColorType.Black);
            if (toPlayer)
            {
                await player.SendChatToPlayerAsync($"{text}");
            }
            else
            {
                await player.SendChatAsync($"{text}");
            }
            await player.SetColorAsync(playercolor);
            await player.SetNameAsync(playername);
        }
    }
}