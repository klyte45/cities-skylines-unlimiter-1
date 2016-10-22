using System;
using System.Collections;
using EightyOne.Areas;

namespace EightyOne
{
    public static class UnlockAllCheat
    {
        public static volatile bool CheatInProgress = false;

        public static void UnlockAllAreas()
        {
            if (!GameAreaManager.exists || !SimulationManager.exists || LoadingManager.instance.m_loadedEnvironment == null)
            {
                return;
            }
            SimulationManager.instance.AddAction(() =>
            {
                GameAreaManager.instance.StartCoroutine(UnlockAllCoroutine());
            });
        }

        private static IEnumerator UnlockAllCoroutine()
        {
            var instance = GameAreaManager.instance;

            for (var i = 0; i < FakeGameAreaManager.GRID; ++i)
            {
                for (var j = 0; j < FakeGameAreaManager.GRID; ++j)
                {
                    var i1 = i;
                    var j1 = j;
                    SimulationManager.instance.AddAction(() =>
                    {
                        try
                        {
                            if (instance.IsUnlocked(i1, j1))
                            {
                                return;
                            }
                            var areaIndex = FakeGameAreaManager.GetTileIndex(i1, j1);
                            //This method gets inlined and can't be detoured
                            CheatInProgress = true;
                            instance.UnlockArea(areaIndex);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                        finally
                        {
                            CheatInProgress = false;
                        }
                    });
                    yield return null;
                }
            }
        }
    }
}