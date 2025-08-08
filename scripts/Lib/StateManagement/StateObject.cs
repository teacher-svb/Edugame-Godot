namespace TnT.Systems.State
{
    public interface IStateObject<T> where T : new()
    {
        public BaseState GetState(T options = default);
    }
}