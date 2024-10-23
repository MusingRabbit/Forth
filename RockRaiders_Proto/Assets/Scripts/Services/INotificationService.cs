using Assets.Scripts.Events;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public interface INotificationService
    {
        event EventHandler<OnNotificationEventArgs> OnError;

        void Error(Exception ex, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "");
        void Error(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "");
        void Info(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "");
        void NotifyPlayerKilled(GameObject playerActor);
        void Warning(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "");
    }
}