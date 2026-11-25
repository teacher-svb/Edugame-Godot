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
    /// - The backtick key ` (under Esc) opens a list of the checkpoints (while nothing else is open: no dialogue, challenge or card). Number keys
    ///   1-9 (or arrows + Enter, or a click) pick one; Escape or ` closes it. Not F9: a game run from the editor
    ///   treats F9/F10 as the editor's Suspend/Next Frame shortcuts and freezes.
    /// - Launch argument <c>--checkpoint=&lt;id&gt;</c> (after <c>--</c> on the command line, or in Project Settings &gt;
    ///   Editor &gt; Run &gt; Main Run Args) jumps there right after the start menu appears.
    /// A jump replaces the current game in memory (nothing is written to disk until the game saves).
    /// Dev tooling: reads the ` key directly and builds its own small UI, outside InputManager and MVC.
    /// </summary>
    [GlobalClass]
    public partial class DevShortcuts : Node
    {
        [Export] public Array<DevCheckpoint> Checkpoints { get; set; } = [];

        CanvasLayer _menu;

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
                else if (!GetTree().Paused) OpenMenu();   // paused = a dialogue, challenge, card or loading screen is up
                GetViewport().SetInputAsHandled();
            }
            else if (key.Keycode == Key.Escape && _menu != null)
            {
                CloseMenu();
                GetViewport().SetInputAsHandled();
            }
            else if (_menu != null && key.Keycode >= Key.Key1 && key.Keycode <= Key.Key9)
            {
                // number keys pick a checkpoint directly (1 = the first)
                var list = Checkpoints.Where(c => c != null).ToList();
                int i = (int)(key.Keycode - Key.Key1);
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
                Name = save.GameData?.Name,
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
            // the quests in memory too: the new level's listeners check them on _Ready, before the save data is bound
            // (jumping back from a later checkpoint would otherwise run their reactions from the old states)
            QuestManager.Instance.Restore(save.GameData.questData);
            GD.Print($"DevShortcuts: jump to '{checkpoint.Id}' ({completed.Count} quests completed) → {checkpoint.Scene} {checkpoint.Spawn}");
            StateManagerGame.Instance.LoadScene(checkpoint.Scene, new NodePath(checkpoint.Spawn));
        }

        void OpenMenu()
        {
            _menu = new CanvasLayer { Layer = 128, ProcessMode = ProcessModeEnum.Always };
            var panel = new PanelContainer { AnchorLeft = 0.5f, AnchorTop = 0.5f, AnchorRight = 0.5f, AnchorBottom = 0.5f };
            var box = new VBoxContainer();
            box.AddChild(new Label { Text = "Dev: jump to checkpoint (1-9 or click, Esc or ` closes)" });
            foreach (var c in Checkpoints.Where(c => c != null))
            {
                int n = box.GetChildCount();   // the label is child 0, so the first button gets 1
                var button = new Button { Text = $"{n}  {(string.IsNullOrEmpty(c.Title) ? c.Id : c.Title)}", Alignment = HorizontalAlignment.Left };
                button.Pressed += () => { CloseMenu(); Jump(c); };
                box.AddChild(button);
            }
            panel.AddChild(box);
            _menu.AddChild(panel);
            AddChild(_menu);
            panel.GrowHorizontal = Control.GrowDirection.Both;
            panel.GrowVertical = Control.GrowDirection.Both;
            GetTree().Paused = true;
            box.GetChildren().OfType<Button>().FirstOrDefault()?.GrabFocus();
        }

        void CloseMenu()
        {
            _menu?.QueueFree();
            _menu = null;
            GetTree().Paused = false;
        }
    }
}
