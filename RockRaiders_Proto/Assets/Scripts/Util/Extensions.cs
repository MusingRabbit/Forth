using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class Extensions
    {
        public static GameObject QueryParents(this GameObject gameObject, Func<GameObject, bool> func)
        {
            if (func(gameObject))
            {
                return gameObject;
            }

            if (gameObject.transform.parent != null)
            {
                return QueryParents(gameObject.transform.parent.gameObject, func);
            }

            return null;
        }

        public static GameObject FindParent(this GameObject gameObject, string name)
        {
            if (gameObject.name == name)
            {
                return gameObject;
            }

            while (gameObject.transform.parent != null)
            {
                var parent = gameObject.transform.parent.gameObject;

                if (parent != null)
                {
                    FindParent(parent, name);
                }
            }

            return null;
        }

        public static List<GameObject> GetAllChildren(this GameObject gameObject, string[] exl)
        {
            return GetAllChildObjects(gameObject, exl);
        }

        public static List<GameObject> GetAllChildObjects(GameObject gameObject, string[] exl)
        {
            var result = new List<GameObject>();

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);

                if (exl.Contains(child.name))
                {
                    continue;
                }

                result.Add(child.gameObject);

                if (child.transform.childCount > 0)
                {
                    result.AddRange(GetAllChildObjects(child.gameObject, exl));
                }
            }

            return result;
        }


        /// <summary>
        /// This search is likely to be slow - so only use it at initialisation. Not at runtime.
        /// NOT AT RUNTIME!!!!
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject FindChild(this GameObject gameObject, string name)
        {
            if (name.Contains('.'))
            {
                return FindChild_ByPath(gameObject, name);
            }

            return Find_By_DepthFirstSearch(gameObject, name);
        }

        private static GameObject Find_By_DepthFirstSearch(GameObject gameObject, string name)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);

                if (child.name == name)
                {
                    return child.gameObject;
                }

                var obj = Find_By_DepthFirstSearch(child.gameObject, name);

                if (obj != null)
                    return obj;
            }

            return null;
        }

        private static GameObject FindChild_ByPath(GameObject gameObject, string name)
        {
            var tokens = name.Split('.');

            var currObj = gameObject.transform.Find(tokens[0])?.gameObject;

            if (tokens.Length == 1)
            {
                return currObj;
            }

            var newPath = string.Join(".", tokens.Skip(1));

            return FindChild_ByPath(currObj, newPath);
        }
    }
}
