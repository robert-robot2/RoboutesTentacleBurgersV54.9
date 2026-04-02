using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace RoboutesTentacleBurgers.Services
{
    public class BloodInput
    {
        public readonly BloodWyrmService Game;
        public readonly IJSRuntime JS;

        // Single key / modifier combo binds
        private readonly Dictionary<string, Func<Task>> binds = new();

        // Sequence binds with advanced features
        private readonly List<SequenceBind> sequenceBinds = new();

        // Tracks recent keys for sequences
        private readonly List<RecentKey> recentKeys = new();

        // Default global sequence timeout
        private readonly TimeSpan DefaultSequenceTimeout = TimeSpan.FromSeconds(2);

        // Mouse state tracking
        private bool isRightMouseDown = false;
        private double lastMouseX = 0;
        private double lastMouseY = 0;

        // Gamepad state tracking
        private readonly HashSet<string> pressedButtons = new();

        public BloodInput(BloodWyrmService game, IJSRuntime js)
        {
            Game = game;
            JS = js;
            RegisterDefaultBinds();
        }

        /* ─────────────── JS REGISTRATION ─────────────── */
        public async Task Register()
        {
            await JS.InvokeVoidAsync(
                "BloodInputAPI.init",
                DotNetObjectReference.Create(this)
            );
        }

        /* ─────────────── BIND SYSTEM ─────────────── */
        public void Bind(string key, Func<Task> action)
        {
            binds[NormalizeKey(key)] = action;
        }

        public void BindSequence(
            string[] keys,
            Func<Task> action,
            TimeSpan? timeout = null,
            TimeSpan? cooldown = null)
        {
            sequenceBinds.Add(new SequenceBind
            {
                Keys = keys.Select(NormalizeKey).ToArray(),
                Action = action,
                Timeout = timeout ?? DefaultSequenceTimeout,
                LastTriggered = DateTime.MinValue,
                Cooldown = cooldown ?? TimeSpan.Zero,
                Progress = 0
            });
        }

        public void Unbind(string key)
        {
            binds.Remove(NormalizeKey(key));
        }

        /* ─────────────── EXECUTE KEY / SEQUENCE ─────────────── */
        private async Task Execute(string key)
        {
            Console.WriteLine($"[INPUT] Execute called with key: '{key}'");
            Console.WriteLine($"[INPUT] Total binds registered: {binds.Count}");

            var now = DateTime.UtcNow;

            // Single key / combo binds
            if (binds.TryGetValue(key, out var action))
            {
                Console.WriteLine($"[INPUT] ✅ FOUND binding for: '{key}'");
                await action.Invoke();
            }
            else
            {
                Console.WriteLine($"[INPUT] ❌ NO BINDING for: '{key}'");
                Console.WriteLine($"[INPUT] Available bindings: {string.Join(", ", binds.Keys.Select(k => $"'{k}'"))}");
            }

            // Add key to recent keys with timestamp
            recentKeys.Add(new RecentKey { Key = key, Time = now });

            // Remove old keys outside the longest timeout
            var maxTimeout = sequenceBinds.Any() ? sequenceBinds.Max(s => s.Timeout) : DefaultSequenceTimeout;
            recentKeys.RemoveAll(k => now - k.Time > maxTimeout);

            // Update sequence progress and trigger if complete
            foreach (var seqBind in sequenceBinds)
            {
                if (now - seqBind.LastTriggered < seqBind.Cooldown) continue;

                int seqLength = seqBind.Keys.Length;
                seqBind.Progress = 0;

                // Only check if we have enough keys
                if (recentKeys.Count >= 1)
                {
                    // Scan all possible start positions
                    for (int start = 0; start <= recentKeys.Count - 1; start++)
                    {
                        int progress = 0;

                        for (int i = 0; i < seqLength && (start + i) < recentKeys.Count; i++)
                        {
                            if (seqBind.Keys[i] == recentKeys[start + i].Key)
                                progress++;
                            else
                                break;
                        }

                        // Update the combo meter with max progress seen
                        if (progress > seqBind.Progress)
                            seqBind.Progress = progress;

                        if (progress == seqLength)
                        {
                            // Sequence completed!
                            await seqBind.Action.Invoke();
                            seqBind.LastTriggered = now;

                            // Do NOT remove keys; allow overlapping sequences
                            break;
                        }
                    }
                }
            }
        }

        /* ─────────────── KEYBOARD HANDLERS ─────────────── */
        public async Task HandleKeyDown(KeyboardEventArgs e)
        {
            Console.WriteLine($"[INPUT] RAW Key pressed: '{e.Key}'");
            var key = BuildKey(e);
            Console.WriteLine($"[INPUT] Built key: '{key}'");
            var normalizedKey = NormalizeKey(key);
            Console.WriteLine($"[INPUT] Normalized key: '{normalizedKey}'");
            await Execute(normalizedKey);
        }

        public async Task HandleKeyUp(KeyboardEventArgs e)
        {
            var key = BuildKey(e);

            // Stop movement if it's a movement key - always fetch from Game
            if (KeybindConfig.IsMovementKey(e.Key))
            {
                Game.CharacterHandle?.ActiveCharacter?.StopMovement(e.Key);
            }

            await Task.CompletedTask;
        }

        /* ─────────────── MOUSE HANDLERS (PLACEHOLDERS) ─────────────── */
        public async Task HandleMouseDown(MouseEventArgs e)
        {
            if (e.Button == 2) // Right mouse button
            {
                isRightMouseDown = true;
                lastMouseX = e.ClientX;
                lastMouseY = e.ClientY;

                // PLACEHOLDER: Future right-click movement
                // Game.StartRightClickMove(e.ClientX, e.ClientY);
            }
            await Task.CompletedTask;
        }

        public async Task HandleMouseUp(MouseEventArgs e)
        {
            if (e.Button == 2)
            {
                isRightMouseDown = false;

                // PLACEHOLDER: Stop right-click movement
                // Game.StopRightClickMove();
            }
            await Task.CompletedTask;
        }

        public async Task HandleMouseMove(MouseEventArgs e)
        {
            if (isRightMouseDown)
            {
                double deltaX = e.ClientX - lastMouseX;
                double deltaY = e.ClientY - lastMouseY;

                // PLACEHOLDER: Future camera look or right-click move
                // Game.Camera?.Look((float)deltaX, (float)deltaY);

                lastMouseX = e.ClientX;
                lastMouseY = e.ClientY;
            }

            // PLACEHOLDER: Viewport edge scrolling
            // CheckViewportEdgeScroll(e.ClientX, e.ClientY);

            await Task.CompletedTask;
        }

        /* ─────────────── GAMEPAD HANDLER ─────────────── */
        [JSInvokable]
        public async Task UpdateGamepadState(GamepadState state)
        {
            // DEBUG: Confirm C# is being reached at all
            Console.WriteLine("[BWP-GP] UpdateGamepadState HIT");
            Console.WriteLine($"[BWP-GP] ActiveCharacter is: {(Game.CharacterHandle?.ActiveCharacter == null ? "NULL" : "SET")}");

            if (Game.CharacterHandle?.ActiveCharacter == null)
            {
                Console.WriteLine("[BWP-GP] BAILING - character null");
                return;
            }
            // Always fetch from Game
        

            // Movement from left stick
            HandleGamepadMovement(state);

            // Buttons
            await HandleGamepadButtons(state);
        }

        private void HandleGamepadMovement(GamepadState state)
        {
            const float deadZone = 0.3f;

            // Always fetch from Game
            var character = Game.CharacterHandle?.ActiveCharacter;
            if (character == null) return;

            string? moveKey = null;

            if (Math.Abs(state.LeftStickX) > deadZone || Math.Abs(state.LeftStickY) > deadZone)
            {
                if (Math.Abs(state.LeftStickX) > Math.Abs(state.LeftStickY))
                {
                    moveKey = state.LeftStickX > 0
                        ? KeybindConfig.GetBinding("MoveRight")
                        : KeybindConfig.GetBinding("MoveLeft");
                }
                else
                {
                    moveKey = state.LeftStickY > 0
                        ? KeybindConfig.GetBinding("MoveDown")
                        : KeybindConfig.GetBinding("MoveUp");
                }

                character.CharMove(moveKey);
            }
            else
            {
                // Stop all movement
                character.StopMovement("w");
                character.StopMovement("a");
                character.StopMovement("s");
                character.StopMovement("d");
            }
        }

        private async Task HandleGamepadButtons(GamepadState state)
        {
            // A Button (Attack)
            if (state.ButtonA && !pressedButtons.Contains("A"))
            {
                pressedButtons.Add("A");
                Game.ExecutePunch();
            }
            else if (!state.ButtonA && pressedButtons.Contains("A"))
            {
                pressedButtons.Remove("A");
            }

            // B Button (Special Attack)
            if (state.ButtonB && !pressedButtons.Contains("B"))
            {
                pressedButtons.Add("B");
                Game.ExecuteSpecial();
            }
            else if (!state.ButtonB && pressedButtons.Contains("B"))
            {
                pressedButtons.Remove("B");
            }

            // Start Button (Menu)
            if (state.ButtonStart && !pressedButtons.Contains("Start"))
            {
                pressedButtons.Add("Start");
                Game.HandleMainMenu();
            }
            else if (!state.ButtonStart && pressedButtons.Contains("Start"))
            {
                pressedButtons.Remove("Start");
            }

            // Back Button (Inventory)
            if (state.ButtonBack && !pressedButtons.Contains("Back"))
            {
                pressedButtons.Add("Back");
                Game.HandleInv();
            }
            else if (!state.ButtonBack && pressedButtons.Contains("Back"))
            {
                pressedButtons.Remove("Back");
            }

            // Y Button (Performance Overlay)
            if (state.ButtonY && !pressedButtons.Contains("Y"))
            {
                pressedButtons.Add("Y");
                Game.ShowPerformanceOverlay = !Game.ShowPerformanceOverlay;
            }
            else if (!state.ButtonY && pressedButtons.Contains("Y"))
            {
                pressedButtons.Remove("Y");
            }

            await Task.CompletedTask;
        }

        /* ─────────────── KEY BUILDING / NORMALIZATION ─────────────── */
        private string BuildKey(KeyboardEventArgs e)
        {
            string key = e.Key switch
            {
                "Escape" => "ESCAPE",
                "ArrowUp" => "UP",
                "ArrowDown" => "DOWN",
                "ArrowLeft" => "LEFT",
                "ArrowRight" => "RIGHT",
                _ => e.Key.ToUpper()
            };

            List<string> keys = new();
            if (e.CtrlKey) keys.Add("CTRL");
            if (e.ShiftKey) keys.Add("SHIFT");
            if (e.AltKey) keys.Add("ALT");

            keys.Add(key);
            return string.Join("+", keys);
        }

        private string NormalizeKey(string key)
        {
            return key
                .ToUpper()
                .Replace(" ", "")
                .Replace("_", "+")
                .Replace("ESC", "ESCAPE");
        }

        private void RegisterDefaultBinds()
        {
            Console.WriteLine("[INPUT] RegisterDefaultBinds START");

            // Movement (WASD) - Always fetch from Game.CharacterHandle
            var moveUpKey = KeybindConfig.GetBinding("MoveUp").ToUpper();
            Console.WriteLine($"[INPUT] Binding MoveUp to: '{moveUpKey}'");
            Bind(moveUpKey, async () => {
                Console.WriteLine("[INPUT] MoveUp action triggered!");
                var character = Game.CharacterHandle?.ActiveCharacter;
                if (character != null)
                {
                    Console.WriteLine($"[INPUT] Character exists, calling CharMove('w')");
                    character.CharMove("w");
                }
                else
                {
                    Console.WriteLine($"[INPUT] ❌ Character is NULL!");
                }
                await Task.CompletedTask;
            });

            var moveDownKey = KeybindConfig.GetBinding("MoveDown").ToUpper();
            Console.WriteLine($"[INPUT] Binding MoveDown to: '{moveDownKey}'");
            Bind(moveDownKey, async () => {
                Console.WriteLine("[INPUT] MoveDown action triggered!");
                var character = Game.CharacterHandle?.ActiveCharacter;
                if (character != null)
                {
                    Console.WriteLine($"[INPUT] Character exists, calling CharMove('s')");
                    character.CharMove("s");
                }
                else
                {
                    Console.WriteLine($"[INPUT] ❌ Character is NULL!");
                }
                await Task.CompletedTask;
            });

            var moveLeftKey = KeybindConfig.GetBinding("MoveLeft").ToUpper();
            Console.WriteLine($"[INPUT] Binding MoveLeft to: '{moveLeftKey}'");
            Bind(moveLeftKey, async () => {
                Console.WriteLine("[INPUT] MoveLeft action triggered!");
                var character = Game.CharacterHandle?.ActiveCharacter;
                if (character != null)
                {
                    Console.WriteLine($"[INPUT] Character exists, calling CharMove('a')");
                    character.CharMove("a");
                }
                else
                {
                    Console.WriteLine($"[INPUT] ❌ Character is NULL!");
                }
                await Task.CompletedTask;
            });

            var moveRightKey = KeybindConfig.GetBinding("MoveRight").ToUpper();
            Console.WriteLine($"[INPUT] Binding MoveRight to: '{moveRightKey}'");
            Bind(moveRightKey, async () => {
                Console.WriteLine("[INPUT] MoveRight action triggered!");
                var character = Game.CharacterHandle?.ActiveCharacter;
                if (character != null)
                {
                    Console.WriteLine($"[INPUT] Character exists, calling CharMove('d')");
                    character.CharMove("d");
                }
                else
                {
                    Console.WriteLine($"[INPUT] ❌ Character is NULL!");
                }
                await Task.CompletedTask;
            });

            Console.WriteLine($"[INPUT] RegisterDefaultBinds END - Total binds: {binds.Count}");

            // Combat
            Bind(KeybindConfig.GetBinding("Attack").ToUpper(), async () => {
                Game.ExecutePunch();
                await Task.CompletedTask;
            });

            Bind(KeybindConfig.GetBinding("SPAttack").ToUpper(), async () => {
                Game.ExecuteSpecial();
                await Task.CompletedTask;
            });

            // UI
            Bind(KeybindConfig.GetBinding("Menu").ToUpper(), async () => {
                Game.HandleMainMenu();
                await Task.CompletedTask;
            });

            Bind(KeybindConfig.GetBinding("Inventory").ToUpper(), async () => {
                Game.HandleInv();
                await Task.CompletedTask;
            });

            Bind(KeybindConfig.GetBinding("Performance").ToUpper(), async () => {
                Game.ShowPerformanceOverlay = !Game.ShowPerformanceOverlay;
                await Task.CompletedTask;
            });

            // Example combos
            BindSequence(new[] { "UP", "UP" }, async () => {
                Console.WriteLine("Double UP combo!");
                await Task.CompletedTask;
            });

            BindSequence(new[] { "F", "F" }, async () => {
                Console.WriteLine("Double attack combo!");
                await Task.CompletedTask;
            }, cooldown: TimeSpan.FromSeconds(1));
        }

        /* ─────────────── DEBUG / COMBO METER ─────────────── */
        public int GetComboProgressPercent()
        {
            if (!sequenceBinds.Any()) return 0;
            var maxSeq = sequenceBinds.OrderByDescending(s => s.Progress).First();
            return maxSeq.Keys.Length == 0 ? 0 : (int)(100.0 * maxSeq.Progress / maxSeq.Keys.Length);
        }

        public List<string> DebugRecentKeys => recentKeys
            .Skip(Math.Max(0, recentKeys.Count - 10))
            .Select(k => k.Key)
            .ToList();

        public List<string> DebugActiveSequences()
        {
            return sequenceBinds
                .Where(s => s.Progress > 0)
                .Select(s => string.Join(" + ", s.Keys))
                .ToList();
        }

        /* ─────────────── HELPER CLASSES ─────────────── */
        private class RecentKey
        {
            public string Key { get; set; } = string.Empty;
            public DateTime Time { get; set; }
        }

        private class SequenceBind
        {
            public string[] Keys { get; set; } = Array.Empty<string>();
            public Func<Task> Action { get; set; } = default!;
            public TimeSpan Timeout { get; set; }
            public DateTime LastTriggered { get; set; }
            public TimeSpan Cooldown { get; set; }
            public int Progress { get; set; } = 0;
        }

        /* ─────────────── GAMEPAD STATE CLASS ─────────────── */
        public class GamepadState
        {
            public float LeftStickX { get; set; }
            public float LeftStickY { get; set; }
            public float RightStickX { get; set; }
            public float RightStickY { get; set; }
            public bool ButtonA { get; set; }
            public bool ButtonB { get; set; }
            public bool ButtonX { get; set; }
            public bool ButtonY { get; set; }
            public bool LeftBumper { get; set; }
            public bool RightBumper { get; set; }
            public float LeftTrigger { get; set; }
            public float RightTrigger { get; set; }
            public bool ButtonBack { get; set; }
            public bool ButtonStart { get; set; }
            public bool LeftStickButton { get; set; }
            public bool RightStickButton { get; set; }
            public bool DpadUp { get; set; }
            public bool DpadDown { get; set; }
            public bool DpadLeft { get; set; }
            public bool DpadRight { get; set; }
        }
    }
}