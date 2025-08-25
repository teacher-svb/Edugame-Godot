
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace TnT.Systems.UI
{
    [Serializable]
    public class MessageModel
    {


        public Queue<Message> messages = new();

        public void Clear() => messages.Clear();
    }

    public class Message
    {
        public enum MessageType
        {
            dialog,
            warning,
            error
        }
        public string text;
        public string name;
        public Sprite sprite;
        public MessageType Type;
    }
}
