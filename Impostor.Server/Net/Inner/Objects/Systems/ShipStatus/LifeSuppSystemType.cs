﻿using System.Collections.Generic;
using Impostor.Api.Events.Managers;
using Impostor.Api.Net.Messages;

namespace Impostor.Server.Net.Inner.Objects.Systems.ShipStatus
{
    public class LifeSuppSystemType : ISystemType, IActivatable
    {
        public LifeSuppSystemType()
        {
            Countdown = 10000f;
            CompletedConsoles = new HashSet<int>();
        }

        public float Countdown { get; private set; }

        public HashSet<int> CompletedConsoles { get; }

        public bool IsActive => Countdown < 10000.0;

        public void Serialize(IMessageWriter writer, bool initialState)
        {
            throw new System.NotImplementedException();
        }

        public void Deserialize(IMessageReader reader, bool initialState, IEventManager eventManager)
        {
            Countdown = reader.ReadSingle();

            if (reader.Position >= reader.Length)
            {
                return;
            }

            CompletedConsoles.Clear(); // TODO: Thread safety

            var num = reader.ReadPackedInt32();

            for (var i = 0; i < num; i++)
            {
                CompletedConsoles.Add(reader.ReadPackedInt32());
            }
        }
    }
}