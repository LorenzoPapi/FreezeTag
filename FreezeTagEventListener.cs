using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Impostor.Api.Innersloth;
using Impostor.Api.Innersloth.Customization;
using Impostor.Api.Net;
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
        private readonly Dictionary<IGame, FreezeTagInfos> CodeAndInfos = new Dictionary<IGame, FreezeTagInfos>();
        private readonly ILogger<FreezeTagPlugin> _logger;

        public FreezeTagEventListener(ILogger<FreezeTagPlugin> logger)
        {
            _logger = logger;
        }

        [EventListener]
        public async void OnGameStarting(IGameStartingEvent e)
        {
            e.Game.Options.KillCooldown = int.MaxValue;
            e.Game.Options.NumEmergencyMeetings = 0;
            await e.Game.SyncSettingsAsync();

        }

        [EventListener]
        public async void OnGameStarted(IGameStartedEvent e)
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

        [EventListener]
        public async void OnPlayerMovement(IPlayerMovementEvent e)
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
                            await Unfreeze(frozen);
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
            CodeAndInfos[e.Game].frozens.Clear();
            CodeAndInfos[e.Game].impostors.Clear();
            CodeAndInfos.Remove(e.Game);
        }

        [EventListener]
        public async void OnPlayerDeath(IPlayerMurderEvent e)
        {
            foreach (var impostor in CodeAndInfos[e.Game].impostors)
            {   
                await impostor.KickAsync();
            }
        }
    }
}