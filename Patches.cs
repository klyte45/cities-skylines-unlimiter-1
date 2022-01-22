using System;
using CitiesHarmony.API;
using EightyOne.HarmonyPatches;
using EightyOne.RedirectionFramework;
using UnityEngine;

namespace EightyOne
{
    internal static class Patches
    {
        private static Tuple<Type, string>[] TypeMethods = {
            Tuple.New(typeof(BusAI), nameof(BusAI.ArriveAtDestination)),
            Tuple.New(typeof(BusAI), "ArriveAtTarget"),
            Tuple.New(typeof(CinematicCameraController), "IsInsideGameAreaLimits"),
            Tuple.New(typeof(EarthquakeAI), "IsStillClearing"),
            //TODO: GameAreaManager ?
            Tuple.New(typeof(MeteorStrikeAI), "IsStillClearing"),            
            Tuple.New(typeof(PassengerPlaneAI), "ArriveAtTarget"),       
            Tuple.New(typeof(PassengerShipAI), "ArriveAtTarget"),      
            Tuple.New(typeof(PassengerTrainAI), "ArriveAtTarget"),
            Tuple.New(typeof(TramAI), "ArriveAtTarget"),  
            //TODO: TransferManager ?
            Tuple.New(typeof(TrolleybusAI), "ArriveAtTarget"),  
            Tuple.New(typeof(TsunamiAI), "IsStillActive"),
            Tuple.New(typeof(TsunamiAI), "IsStillClearing"),
            Tuple.New(typeof(TsunamiAI), "IsStillEmerging"),
        };

        private static Type[] StartPathFindTypes =
        {
            typeof(CargoTruckAI),
            typeof(PostVanAI)
        };

        internal static void Apply()
        {
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
            foreach (var type in StartPathFindTypes)
            {
                MaxDistanceTranspiler.Apply(type, "StartPathFind", new[]
                    {
                        typeof(ushort),
                        typeof(Vehicle).MakeByRefType(),
                        typeof(Vector3),
                        typeof(Vector3),
                        typeof(bool),
                        typeof(bool),
                        typeof(bool)
                    });       
            }
            foreach (var tuple in TypeMethods)
            {
                MaxDistanceTranspiler.Apply(tuple.First, tuple.Second);
            }
        }

        internal static void Undo()
        {
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
            foreach (var type in StartPathFindTypes)
            {
                MaxDistanceTranspiler.Undo(type, "StartPathFind", new[]
                {
                    typeof(ushort),
                    typeof(Vehicle).MakeByRefType(),
                    typeof(Vector3),
                    typeof(Vector3),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool)
                });       
            }
            foreach (var tuple in TypeMethods)
            {
                MaxDistanceTranspiler.Undo(tuple.First, tuple.Second);
            }
        }
    }
}