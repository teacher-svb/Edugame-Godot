using System;
using Godot;

namespace TnT.Systems
{
    /// <summary>
    /// Quits without the .NET finalizer racing the engine's shutdown. A C# wrapper of a Godot object (a Resource, a
    /// Tween, ...) that is garbage but not finalized yet when the engine shuts down is not disposed by Godot; its
    /// finalizer then runs on the .NET finalizer thread while the engine frees its object database, and can crash
    /// the process (SIGSEGV → SIGABRT, a core dump at exit). Freeing the scene tree frees every node, which makes the
    /// game's references to resources garbage at once (e.g. the quests a manager holds). So before quitting this
    /// frees the root's children, lets the frees happen, runs the garbage collector and waits for the finalizers
    /// while the engine is still fully up, then quits. Seen in headless runs after a longer session (2026-11-25):
    /// "125 ObjectDB instances were leaked at exit" and a crash at exit in 3 of 4 runs; with this, no leak and no
    /// crash in 8 runs.
    /// </summary>
    /// <remarks>
    /// <see cref="Install"/> routes the window's close button through it. Call <see cref="Quit"/> instead of
    /// <c>SceneTree.Quit()</c>. Engine-side quits (<c>--quit-after</c>, a bare <c>Quit()</c>) still take the old way.
    /// </remarks>
    public static class CleanQuit
    {
        static bool _quitting;

        /// <summary>The window's close button quits through <see cref="Quit"/> (instead of the engine's auto quit).</summary>
        public static void Install(SceneTree tree)
        {
            tree.AutoAcceptQuit = false;
            tree.Root.CloseRequested += () => Quit(tree);
        }

        /// <summary>Frees the scene tree, lets the finalizers run, then quits with <paramref name="exitCode"/>.</summary>
        public static async void Quit(SceneTree tree, int exitCode = 0)
        {
            if (_quitting || tree == null)
                return;
            _quitting = true;
            try
            {
                // the frees happen at the end of the frame; in each of two frames, so a scene that a scene change
                // still pending at the quit (during a loading screen) adds meanwhile is freed too
                for (int i = 0; i < 2; i++)
                {
                    foreach (var node in tree.Root.GetChildren())
                        node.QueueFree();
                    await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                }
                for (int i = 0; i < 2; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                // a short real-time wait, for anything released deferred: the finalizers' releases, and a stopped
                // sound, which the audio thread lets go of only on its next mix (a playing track otherwise shows up
                // as "2 ObjectDB instances were leaked at exit": AudioStreamWAV + its playback)
                await tree.ToSignal(tree.CreateTimer(0.1, processAlways: true, processInPhysics: false, ignoreTimeScale: true),
                    SceneTreeTimer.SignalName.Timeout);
            }
            catch (Exception e)
            {
                GD.PushError($"CleanQuit: {e}");
            }
            tree.Quit(exitCode);
        }
    }
}
