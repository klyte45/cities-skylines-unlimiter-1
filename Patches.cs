using System;
using EightyOne.HarmonyPatches;
using EightyOne.RedirectionFramework;

namespace EightyOne
{
    public static class Patches
    {
        private static Tuple<Type, string>[] TypeMethods = {
            new(typeof(BusAI), nameof(BusAI.ArriveAtDestination)),
            new(typeof(BusAI), "ArriveAtTarget"),
            //TODO: CargoTruckAI
            new(typeof(CinematicCameraController), "IsInsideGameAreaLimits"),
            new(typeof(EarthquakeAI), "IsStillClearing"),
            //TODO: GameAreaManager ?
            new(typeof(MeteorStrikeAI), "IsStillClearing"),            
            new(typeof(PassengerPlaneAI), "ArriveAtTarget"),       
            new(typeof(PassengerShipAI), "ArriveAtTarget"),      
            new(typeof(PassengerTrainAI), "ArriveAtTarget"),  
            //TODO: PostVanAI
            new(typeof(TramAI), "ArriveAtTarget"),  
            //TODO: TransferManager ?
            new(typeof(TrolleybusAI), "ArriveAtTarget"),  
            new(typeof(TsunamiAI), "IsStillActive"),
            new(typeof(TsunamiAI), "IsStillClearing"),
            new(typeof(TsunamiAI), "IsStillEmerging"),
        };
        
        public static void Apply()
        {

            foreach (var tuple in TypeMethods)
            {
                MaxDistanceTranspiler.Apply(tuple.First, tuple.Second);
            }
        }

        public static void Undo()
        {
            foreach (var tuple in TypeMethods)
            {
                MaxDistanceTranspiler.Undo(tuple.First, tuple.Second);
            }
        }
    }
}