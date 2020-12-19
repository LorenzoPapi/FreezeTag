﻿using Impostor.Api.Events.Managers;
using Impostor.Api.Net.Messages;

namespace Impostor.Server.Net.Inner.Objects.Systems
{
    public interface ISystemType
    {
        void Serialize(IMessageWriter writer, bool initialState);

        void Deserialize(IMessageReader reader, bool initialState, IEventManager eventManager);
    }
}