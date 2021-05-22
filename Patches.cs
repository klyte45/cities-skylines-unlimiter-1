using System;
using EightyOne.HarmonyPatches;
using EightyOne.RedirectionFramework;
using UnityEngine;

namespace EightyOne
{
    internal static class Patches
    {
        private static Tuple<Type, string>[] TypeMethods = {
            new(typeof(BusAI), nameof(BusAI.ArriveAtDestination)),
            new(typeof(BusAI), "ArriveAtTarget"),
            new(typeof(CinematicCameraController), "IsInsideGameAreaLimits"),
            new(typeof(EarthquakeAI), "IsStillClearing"),
            //TODO: GameAreaManager ?
            new(typeof(MeteorStrikeAI), "IsStillClearing"),            
            new(typeof(PassengerPlaneAI), "ArriveAtTarget"),       
            new(typeof(PassengerShipAI), "ArriveAtTarget"),      
            new(typeof(PassengerTrainAI), "ArriveAtTarget"),  
            new(typeof(TramAI), "ArriveAtTarget"),  
            //TODO: TransferManager ?
            new(typeof(TrolleybusAI), "ArriveAtTarget"),  
            new(typeof(TsunamiAI), "IsStillActive"),
            new(typeof(TsunamiAI), "IsStillClearing"),
            new(typeof(TsunamiAI), "IsStillEmerging"),
        };

        private static Type[] StartPathFindTypes =
        {
            typeof(CargoTruckAI),
            typeof(PostVanAI)
        };

        internal static void Apply()
        {
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