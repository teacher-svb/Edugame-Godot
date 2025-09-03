using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using TnT.Extensions;
using Newtonsoft.Json;

namespace TnT.Systems.Persistence
{
    [GlobalClass]
    public abstract partial class GameData : Resource
    {
        [Export] public string Name;
    }
}