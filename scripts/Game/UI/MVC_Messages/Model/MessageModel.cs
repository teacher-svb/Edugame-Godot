
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.Systems.UI
{
    [GlobalClass]
    public partial class MessageModel : Resource
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
        public Texture2D sprite;
        public MessageType Type;
    }
}
