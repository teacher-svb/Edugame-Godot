namespace TnT.Input
{
    public interface IInputActionable
    {
        InputActionBase[] InputActions { get; }

        /// <summary>
        /// Polls all actions in <see cref="InputActions"/>. Override to add custom polling logic. An action that
        /// throws is logged (once per action) and the others are still polled.
        /// </summary>
        void Poll()
        {
            foreach (var action in InputActions)
                InputManager.PollSafely(action);
        }

        /// <summary>Registers this node with <see cref="InputManager"/> to be polled each frame.</summary>
        void Register()   => InputManager.Instance?.Register(this);

        /// <summary>Unregisters this node from <see cref="InputManager"/>.</summary>
        void Unregister() => InputManager.Instance?.Unregister(this);
    }
}
