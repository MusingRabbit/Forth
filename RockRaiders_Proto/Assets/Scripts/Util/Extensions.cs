using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class Extensions
    {
        public static bool QueryParents(this GameObject gameObject, Func<GameObject, bool> func)
        {
            if (func(gameObject))
            {
                return true;
            }

            if (gameObject.transform.parent != null)
            {
                return QueryParents(gameObject.transform.parent.gameObject, func);
            }

            return false;
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
