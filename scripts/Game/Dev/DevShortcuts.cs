using System.Linq;
using Godot;
using Godot.Collections;
using TnT.EduGame.GameStates;
using TnT.EduGame.QuestSystem;

namespace TnT.EduGame.Dev
{
    /// <summary>
    /// Dev shortcuts, debug builds only (the node frees itself in a release export): jump to a
    /// <see cref="DevCheckpoint"/> instead of playing up to it.
    /// - The backtick key ` (under Esc) opens a list of the checkpoints (while nothing else is open: only the start menu or
    ///   the game itself, no dialogue, menu, challenge, card or load). The menu is a state on the game's state stack,
    ///   so the play state below it gets no input (Escape closes only the menu). Number keys
    ///   1-9 (or arrows + Enter, or a click) pick one; Escape or ` closes it. Not F9: a game run from the editor
    ///   treats F9/F10 as the editor's Suspend/Next Frame shortcuts and freezes.
    /// - Launch argument <c>--checkpoint=&lt;id&gt;</c> (after <c>--</c> on the command line, or in Project Settings &gt;
    ///   Editor &gt; Run &gt; Main Run Args) jumps there right after the start menu appears.
    /// A jump replaces the current game in memory (nothing is written to disk until the game saves) and runs under its
    /// own save name (<see cref="SaveName"/>), so saving after a jump never overwrites the real save.
    /// Dev tooling: reads the ` key directly and builds its own small UI, outside InputManager and MVC.
    /// </summary>
    [GlobalClass]
    public partial class DevShortcuts : Node
    {
        [Export] public Array<DevCheckpoint> Checkpoints { get; set; } = [];

        /// <summary>
        /// The save a game started by a jump uses (<c>user://dev.json</c>): its autosaves and the pause menu's Save and
        /// Load use this one, the start menu's Load the real save.
        /// </summary>
        public const string SaveName = "dev";

        CanvasLayer _menu;
        // the menu's state on the StateManagerGame stack, until its OnExit ran
        Systems.State.BaseState _menuState;

        public override void _Ready()
        {
            if (!OS.IsDebugBuild())
            {
                QueueFree();
                return;
            }
            ProcessMode = ProcessModeEnum.Always;
            var arg = OS.GetCmdlineUserArgs().FirstOrDefault(a => a.StartsWith("--checkpoint="));
            if (arg != null)
                JumpAfterStart(arg["--checkpoint=".Length..]);
        }

        async void JumpAfterStart(string id)
        {
            // let the start menu (the main scene) come up first, as if New Game was pressed
            await ToSignal(GetTree().CreateTimer(0.5, true, false, true), SceneTreeTimer.SignalName.Timeout);
            var checkpoint = Checkpoints.FirstOrDefault(c => c.Id == id);
            if (checkpoint == null)
            {
                GD.PushWarning($"DevShortcuts: no checkpoint '{id}'. Known: {string.Join(", ", Checkpoints.Select(c => c.Id))}");
                return;
            }
            Jump(checkpoint);
        }

        public override void _UnhandledInput(InputEvent e)
        {
            if (e is not InputEventKey { Pressed: true, Echo: false } key)
                return;
            // backtick (physical key under Esc), not F9/F10: a game run from the editor treats those as the editor's
            // "Suspend/Resume" and "Next Frame" shortcuts and freezes (2026-11-22)
            if (key.PhysicalKeycode == Key.Quoteleft)
            {
                if (_menu != null) CloseMenu();
                else if (StateManagerGame.Instance.IsIdle) OpenMenu();
                GetViewport().SetInputAsHandled();
            }
            else if (key.PhysicalKeycode == Key.Escape && _menu != null)
            {
                CloseMenu();
                GetViewport().SetInputAsHandled();
            }
            else if (_menu != null && key.PhysicalKeycode >= Key.Key1 && key.PhysicalKeycode <= Key.Key9)
            {
                // number keys pick a checkpoint directly (1 = the first); physical keys, so the digit row works on
                // AZERTY too
                var list = Checkpoints.Where(c => c != null).ToList();
                int i = (int)(key.PhysicalKeycode - Key.Key1);
                GetViewport().SetInputAsHandled();
                if (i < list.Count) { CloseMenu(); Jump(list[i]); }
            }
        }

        /// <summary>A fresh game with the checkpoint's quests completed, then its level at its spawn point.</summary>
        public void Jump(DevCheckpoint checkpoint)
        {
            var save = SaveLoadManager.Instance;
            var completed = checkpoint.CompletedQuests.Where(q => q != null).ToList();
            save.GameData = new MyGameData
            {
                Name = SaveName,   // never the real save: an autosave or pause-menu Save after a jump writes here
                CurrentLevelName = checkpoint.Scene,
                questData = new QuestManagerSaveData
                {
                    quests = completed.Select(q => new QuestSaveData
                    {
                        Id = q.Id,
                        Objectives = q.Objectives.Select(o => new QuestObjectiveSaveData { Id = o.ObjectiveId, State = QuestState.COMPLETED }).ToArray(),
                    }).ToArray(),
                },
                tutorialData = checkpoint.TutorialDone ? new TutorialSaveData { IsCompleted = true } : null,
            };
            // the quests in memory too: the new level's listeners catch up from them once the scene is bound
            // (jumping back from a later checkpoint would otherwise run their reactions from the old states)
            QuestManager.Instance.Restore(save.GameData.questData);
            UI.InventoryController.Instance?.model?.Restore(null);   // a fresh game: no items from the running session
            GD.Print($"DevShortcuts: jump to '{checkpoint.Id}' ({completed.Count} quests completed) → {checkpoint.Scene} {checkpoint.Spawn}");
            StateManagerGame.Instance.LoadScene(checkpoint.Scene, new NodePath(checkpoint.Spawn));
        }

        // pushes the menu as a state: shown at once, its OnEnter pauses the game, its OnExit frees it (also when
        // something else removes the state, e.g. a scene load unwinding the stack)
        void OpenMenu()
        {
            var menu = BuildMenu(out var firstButton);
            Systems.State.BaseState state = null;
            state = new Systems.State.BaseState(new()
            {
                OnEnter = () =>
                {
                    GetTree().Paused = true;
                    return System.Threading.Tasks.Task.CompletedTask;
                },
                OnExit = () =>
                {
                    menu.QueueFree();
                    if (_menu == menu)
                        _menu = null;
                    if (_menuState == state)
                        _menuState = null;
                    GetTree().Paused = false;   // Play's resume unpauses too; the start menu (base) has no resume of its own to do it
                    return System.Threading.Tasks.Task.CompletedTask;
                },
            });
            _menu = menu;
            _menuState = state;
            AddChild(menu);
            StateManagerGame.Instance.PushOverlay(state);
            firstButton?.GrabFocus();
        }

        CanvasLayer BuildMenu(out Button firstButton)
        {
            firstButton = null;
            var layer = new CanvasLayer { Layer = 128, ProcessMode = ProcessModeEnum.Always };
            var panel = new PanelContainer { AnchorLeft = 0.5f, AnchorTop = 0.5f, AnchorRight = 0.5f, AnchorBottom = 0.5f };
            var box = new VBoxContainer();
            box.AddChild(new Label { Text = "Dev: jump to checkpoint (1-9 or click, Esc or ` closes)" });
            foreach (var c in Checkpoints.Where(c => c != null))
            {
                int n = box.GetChildCount();   // the label is child 0, so the first button gets 1
                var button = new Button { Text = $"{n}  {(string.IsNullOrEmpty(c.Title) ? c.Id : c.Title)}", Alignment = HorizontalAlignment.Left };
                button.Pressed += () => { CloseMenu(); Jump(c); };
                box.AddChild(button);
                firstButton ??= button;
            }
            panel.AddChild(box);
            layer.AddChild(panel);
            panel.GrowHorizontal = Control.GrowDirection.Both;
            panel.GrowVertical = Control.GrowDirection.Both;
            return layer;
        }

        // pops the menu's state (its OnExit frees the menu); a jump right after pushes the loader on top, and the
        // stack exits the menu state below it first
        void CloseMenu()
        {
            if (_menu == null)
                return;
            _menu.Visible = false;   // gone at once; freed by the state's OnExit
            _menu = null;
            _ = StateManagerGame.Instance.Pop(_menuState);
        }
    }
}
