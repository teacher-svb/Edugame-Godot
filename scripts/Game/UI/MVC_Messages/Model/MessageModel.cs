using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.UI
{
    [GlobalClass]
    public partial class MessageModel : Node
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
        // public Texture2D sprite;
        public string characterScenePath;
        public MessageType Type;
    }
}
