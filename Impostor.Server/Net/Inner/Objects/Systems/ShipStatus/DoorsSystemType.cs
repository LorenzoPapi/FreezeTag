﻿using System;
using System.Collections.Generic;
using System.Text;
using Impostor.Api.Events.Managers;
using Impostor.Api.Games;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Messages;
using Impostor.Server.Events;

namespace Impostor.Server.Net.Inner.Objects.Systems.ShipStatus
{
    public class DoorsSystemType : ISystemType
    {
        // TODO: AutoDoors
        private readonly Dictionary<int, bool> _doors;
        private readonly IGame _game;

        public DoorsSystemType(IGame game)
        {
            _game = game;
            var doorCount = game.Options.Map switch
            {
                MapTypes.Skeld => 13,
                MapTypes.MiraHQ => 2,
                MapTypes.Polus => 12,
                _ => throw new ArgumentOutOfRangeException()
            };

            _doors = new Dictionary<int, bool>(doorCount);

            for (var i = 0; i < doorCount; i++)
            {
                _doors.Add(i, false);
            }
        }

        public void Serialize(IMessageWriter writer, bool initialState)
        {
            throw new NotImplementedException();
        }

        public async void Deserialize(IMessageReader reader, bool initialState, IEventManager manager)
        {
            if (initialState)
            {
                for (var i = 0; i < _doors.Count; i++)
                {
                    _doors[i] = reader.ReadBoolean();
                }
            }
            else
            {
                var num = reader.ReadPackedUInt32();
                for (var i = 0; i < _doors.Count; i++)
                {
                    if ((num & 1 << i) != 0)
                    {
                        _doors[i] = reader.ReadBoolean();
                        await manager.CallAsync(new GameDoorStateChangedEvent(_game, num, _doors[i]));
                    }
                }
            }
        }
    }
}