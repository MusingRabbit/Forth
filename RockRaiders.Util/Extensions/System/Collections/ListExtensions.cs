using System;
using System.Collections.Generic;

namespace RockRaiders.Util.Extensions.System.Collections
{
    public enum ShiftDirection
    {
        Increment,
        Decremenet
    }

    public static class ListExtensions
    {
        /// <summary>
        /// Incremenet / Decrement the order of a particular entry within a Generic List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="target"></param>
        /// <param name="shiftDirection"></param>
        public static void Shift<T>(this List<T> list, T target, ShiftDirection shiftDirection)
            where T : class
        {
            var incremenet = shiftDirection == ShiftDirection.Increment;
            var offset = incremenet ? 1 : -1;

            for (var i = 0; i < list.Count; i++)
            {
                var canSwap = incremenet && i < list.Count - 1 || !incremenet && i > 0;

                if (canSwap)
                {
                    if (target == list[i]) //Do Swap
                    {
                        var tmp = list[i];
                        list[i] = list[i + offset];
                        list[i + offset] = tmp;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Performs a shallow copy of each item <typeparamref name="T"/> within the List
        /// </summary>
        /// <typeparam name="T">collection type</typeparam>
        /// <param name="listToClone">source list</param>
        /// <returns></returns>
        public static List<T> Clone<T>(this List<T> listToClone) where T : class
        {
            var result = new List<T>(listToClone.Capacity);

            for (var i = 0; i < listToClone.Count; i++)
            {
                var obj = Activator.CreateInstance<T>();
                obj.CopyProperties(listToClone[i]).Update();
                result.Add(obj);
            }

            return result;
        }
    }
}
