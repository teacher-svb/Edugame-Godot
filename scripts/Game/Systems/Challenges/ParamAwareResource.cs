using System.Collections.Generic;
using Godot;

namespace TnT.EduGame.Question
{
    // Base for resources with string fields that name a formula param. The owning
    // MathChallenge passes its param names down (SetAvailableParams), so those
    // fields get a dropdown in the inspector instead of free text.
    [Tool]
    public abstract partial class ParamAwareResource : Resource, IParamFieldOwner
    {
        protected string[] AvailableParams { get; private set; } = [];

        // overridden by resources that hold nested param-aware resources, to pass the names on
        public virtual void SetAvailableParams(string[] names)
        {
            AvailableParams = names;
            NotifyPropertyListChanged();
        }

        // names of this resource's string properties that hold a param name
        protected virtual IEnumerable<string> ParamFields => [];

        public override void _ValidateProperty(Godot.Collections.Dictionary property) =>
            ParamFieldHints.Apply(property, ParamFields, AvailableParams);
    }
}
