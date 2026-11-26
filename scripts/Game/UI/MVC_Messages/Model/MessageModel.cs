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
        /// <summary>Who speaks the line (null: a text-only box); turned toward the player when it shows.</summary>
        public TnT.EduGame.Characters.CharacterData character;
        public MessageType Type;
    }
}
