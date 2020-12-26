﻿namespace Impostor.Api.Innersloth
{
    public enum SystemTypes : byte
    {
        Hallway = 0,
        Storage = 1,
        Cafeteria = 2,
        Reactor = 3,
        UpperEngine = 4,
        Nav = 5,
        Admin = 6,
        Electrical = 7,
        LifeSupp = 8,
        Shields = 9,
        MedBay = 10,
        Security = 11,
        Weapons = 12,
        LowerEngine = 13,
        Comms = 14,
        ShipTasks = 15,
        Doors = 16,
        Sabotage = 17,

        /// <summary>
        /// Decontam on Mira and bottom decontam on Polus
        /// </summary>
        Decontamination = 18,
        Launchpad = 19,
        LockerRoom = 20,
        Laboratory = 21,
        Balcony = 22,
        Office = 23,
        Greenhouse = 24,
        Dropship = 25,

        /// <summary>
        /// Top decontam on Polus
        /// </summary>
        Decontamination2 = 26,
        Outside = 27,
        Specimens = 28,
        BoilerRoom = 29,
    }
}