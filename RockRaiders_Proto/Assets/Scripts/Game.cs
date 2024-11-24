using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Game
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void Initialise()
        {
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
                        Debug.unityLogger.filterLogType =  LogType.Warning;
#endif
        }


        public static void Pause()
        {
            Time.timeScale = 0;
        }

        public static void Resume()
        {
            Time.timeScale = 1;
        }
    }
}
