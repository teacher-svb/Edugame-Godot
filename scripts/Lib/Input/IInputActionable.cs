using System.Collections.Generic;
using System.Linq;

namespace TnT.Input
{
    public interface IInputActionable
    {
        InputActionBase[] InputActions { get; }

        /// <summary>Polls all actions returned by <see cref="GetInputActions"/>. Override to add custom polling logic.</summary>
        void Poll()
        {
            foreach (var action in InputActions)
                action.Poll();
        }

        /// <summary>Registers this node with <see cref="InputManager"/> to be polled each frame.</summary>
        void Register()   => InputManager.Instance?.Register(this);

        /// <summary>Unregisters this node from <see cref="InputManager"/>.</summary>
        void Unregister() => InputManager.Instance?.Unregister(this);
    }
}
