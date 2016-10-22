using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EightyOne.Areas;
using UnityEngine;

namespace EightyOne
{
    public static class UnlockAllCheat
    {
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

            var set = GetUnlockables();
            while (set.Length > 0)
            {
                var counter = set.Length;
                foreach (var keyValuePair in set)
                {
                    SimulationManager.instance.AddAction(() =>
                    {
                        if (instance.IsUnlocked(keyValuePair.Key, keyValuePair.Value))
                        {
                            return;
                        }
                        var areaIndex = FakeGameAreaManager.GetTileIndex(keyValuePair.Key, keyValuePair.Value); //This method gets inlined and can't be detoured
                        instance.UnlockArea(areaIndex);
                        Interlocked.Decrement(ref counter);
                    });
                    yield return null;
                }
                while (counter > 0)
                {
                    yield return new WaitForEndOfFrame();
                }
                set = GetUnlockables();
                yield return null;
            }
        }

        private static KeyValuePair<int, int>[] GetUnlockables()
        {
            var instance = GameAreaManager.instance;
            var set = new HashSet<KeyValuePair<int, int>>();
            for (var i = 0; i < FakeGameAreaManager.GRID; ++i)
            {
                for (var j = 0; j < FakeGameAreaManager.GRID; ++j)
                {
                    if (!instance.IsUnlocked(i, j))
                    {
                        continue;
                    }
                    if (!instance.IsUnlocked(i + 1, j) && instance.CanUnlock(i + 1, j))
                    {
                        set.Add(new KeyValuePair<int, int>(i + 1, j));
                    }
                    if (!instance.IsUnlocked(i - 1, j) && instance.CanUnlock(i - 1, j))
                    {
                        set.Add(new KeyValuePair<int, int>(i - 1, j));
                    }
                    if (!instance.IsUnlocked(i, j + 1) && instance.CanUnlock(i, j + 1))
                    {
                        set.Add(new KeyValuePair<int, int>(i, j + 1));
                    }
                    if (!instance.IsUnlocked(i, j - 1) && instance.CanUnlock(i, j - 1))
                    {
                        set.Add(new KeyValuePair<int, int>(i, j - 1));
                    }
                }
            }
            return set.ToArray();
        }
    }
}