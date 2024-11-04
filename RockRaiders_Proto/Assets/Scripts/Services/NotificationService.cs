using Assets.Scripts.Actor;
using Assets.Scripts.Events;
using Assets.Scripts.Pickups.Weapons;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public class MessageData
    {
        private MessageType m_messageType;
        private string m_message;
        private object m_data;

        public string Message
        {
            get
            {
                return m_message;
            }
            set
            {
                m_message = value;
            }
        }

        public MessageType MessageType
        {
            get
            {
                return m_messageType;
            }
            set
            {
                m_messageType = value;
            }
        }

        public MessageData()
        {
            
        }

        public MessageData(object data)
        {
            m_data = data;
        }

        public void SetData<T>(T data)
        {
            m_data = data;
        }

        public T GetData<T>()
        {
            return (T)m_data;
        }
    }

    public class PlayerKilledData
    {
        public GameObject Killed { get; set; }
        public GameObject Killer { get; set; }
    }

    public class NotificationService : INotificationService
    {
        public event EventHandler<OnNotificationEventArgs> OnInfo;
        public event EventHandler<OnNotificationEventArgs> OnWarning;
        public event EventHandler<OnNotificationEventArgs> OnError;


        public event EventHandler<OnNotificationEventArgs> OnPlayerKilled;


        private static NotificationService _instance;
        public static NotificationService Instance
        {
            get
            {
                return _instance = _instance ?? new NotificationService();
            }
        }

        public NotificationService()
        {

        }

        private string GetMessagePrefix(string filePath, string memberName)
        {
            var className = Path.GetFileNameWithoutExtension(filePath);
            return $"{DateTime.Now.ToShortTimeString()}|{className}|{memberName} : ";
        }

        public void Info(object message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            this.Info(message.ToString(), callerFilePath, callerMemberName);
        }

        public void Info(string message = "", [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            var prefix = this.GetMessagePrefix(callerFilePath, callerMemberName);
            var fullMessage = prefix + message;
            Debug.Log(fullMessage);
            this.Notify(MessageType.Info, fullMessage, (object)null);
        }

        public void Warning(object message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            this.Warning(message.ToString(), callerFilePath, callerMemberName);
        }

        public void Warning(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            var prefix = this.GetMessagePrefix(callerFilePath, callerMemberName);
            var fullMessage = prefix + message;
            Debug.LogWarning(fullMessage);
            this.Notify(MessageType.Warning, fullMessage, (object)null);
        }

        public void Error(object message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            this.Error(message.ToString(), callerFilePath, callerMemberName);
        }

        public void Error(Exception ex, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            var prefix = this.GetMessagePrefix(callerFilePath, callerMemberName);
            var fullMessage = prefix + ex.Message;
            Debug.LogError(ex);
            this.Notify(MessageType.Error, fullMessage, ex);
        }

        public void Error(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
        {
            var prefix = this.GetMessagePrefix(callerFilePath, callerMemberName);
            var fullMessage = prefix + message;

            Debug.Log(fullMessage);
            this.Notify(MessageType.Error, fullMessage, (object)null);
        }

        private void Notify<T>(MessageType messageType, string message, T data = default)
        {

            switch (messageType)
            {
                case MessageType.Info:
                    this.OnInfo?.Invoke(this, new OnNotificationEventArgs(new MessageData(data) { Message = message, MessageType = messageType }));
                    break;
                case MessageType.Warning:
                    this.OnWarning?.Invoke(this, new OnNotificationEventArgs(new MessageData(data) { Message = message, MessageType = messageType }));
                    break;
                case MessageType.Error:
                    this.OnError?.Invoke(this, new OnNotificationEventArgs(new MessageData(data) { Message = message, MessageType = messageType }));
                    break;
                case MessageType.PlayerKilled:
                    this.OnPlayerKilled?.Invoke(this, new OnNotificationEventArgs(new MessageData(data) { Message = message, MessageType = messageType }));
                    break;
                case MessageType.PlayerSpawned:
                    break;
            }
        }

        public void NotifyPlayerKilled(GameObject playerActor)
        {
            var state = playerActor.GetComponent<ActorState>();
            var playerName = state.PlayerName ?? "Unknown";
            string messageText = string.Empty;

            var messageData = new PlayerKilledData
            {
                Killed = playerActor
            };

            if (state.LastHitBy != null)
            {
                var weaponObj = state.LastHitBy;

                Debug.Log("Player Killed : Weapon: " + weaponObj.name);

                var rhsActor = weaponObj.GetComponent<Weapon>().Owner;

                Debug.Log("Player Killed : RHSActor: " + rhsActor.name);

                var rhsState = rhsActor.GetComponent<ActorState>();

                Debug.Log("Player Killed : RHSState: " + rhsState?.name + "|" + rhsState.PlayerName);

                var killedBy = rhsState.PlayerName ?? "Unknown";

                messageData.Killer = rhsActor;

                messageText = $"{playerName} killed by {killedBy}";
            }
            else
            {
                messageText = $"{playerName} killed by UNKNOWN";
            }


            Debug.Log(messageText);

            this.Notify(MessageType.PlayerKilled, messageText, messageData);
        }
    }
}
