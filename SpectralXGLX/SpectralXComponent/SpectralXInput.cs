


using SpectralXGLX.SpectralXComponent.SpectralXDebug;

namespace SpectralXGLX.SpectralXComponent
{
    public class SpectralXInput
    {
        public readonly SpectralXGLX SpectralXGLX;
        public readonly IJSRuntime JS;
        public readonly SpectralXCamera Camera;
        public readonly SpectralXViewport Viewport;
        public SpectralXDebugRender Debug { get; set; } = default!;
        // Single key / modifier combo binds
        private readonly Dictionary<string, Func<Task>> binds = new();

        // Sequence binds with advanced features
        private readonly List<SequenceBind> sequenceBinds = new();

        // Tracks recent keys for sequences
        private readonly List<RecentKey> recentKeys = new();

        // Default global sequence timeout
        private readonly TimeSpan DefaultSequenceTimeout = TimeSpan.FromSeconds(2);

        private readonly HashSet<string> _heldKeys = new();

        private int _draggingLightIndex = -1; // -1 = not dragging any light

        private bool _isShiftHeld = false;

        private bool isRightMouseDown = false;
        private bool isLeftMouseDown = false;  // ADD THIS
        private SpectralXEngine? _engine => SpectralXGLX.Engine;
        public void ToggleDebugOverlay()
        {
            Debug.Enabled = !Debug.Enabled;
            
        }



        public SpectralXInput(SpectralXGLX spectralX, SpectralXViewport viewport, SpectralXCamera camera,IJSRuntime js)
        {
            SpectralXGLX = spectralX;  
            Viewport = viewport; 
            Camera = camera; 
            JS = js;            
            RegisterDefaultBinds();
        }


     


        /* ─────────────── JS FULLSCREEN CALLBACK ─────────────── */
        [JSInvokable]
        public async Task OnFullscreenExit()
        {
            if (SpectralXGLX.IsFullscreen)
                await SpectralXGLX.ToggleViewport();
        }

        public async Task Register()
        {
            // Registration handled by SpectralXGLX.OnAfterRenderAsync
            await Task.CompletedTask;
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
            var now = DateTime.UtcNow;

            // ESC always toggles viewport
            if (key.Contains("ESCAPE") && !SpectralXGLX.IsFullscreen)
                await SpectralXGLX.ToggleViewport();

            // Single key / combo binds
            if (binds.TryGetValue(key, out var action))
                await action.Invoke();

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

                            // ✅ Do NOT remove keys; allow overlapping sequences
                            break;
                        }
                    }
                }
            }



        }

        /* ─────────────── INPUT HANDLER ─────────────── */
        public async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.ShiftKey) _isShiftHeld = true;
            var key = BuildKey(e);
            var normalizedKey = NormalizeKey(key);
            _heldKeys.Add(normalizedKey);

            // Movement keys — just track held state, don't run Execute
            if (normalizedKey is "W" or "A" or "S" or "D") return;

            await Execute(normalizedKey);
        }
        public void HandleKeyUp(KeyboardEventArgs e)
        {
            if (!e.ShiftKey) _isShiftHeld = false;
            _heldKeys.Remove(NormalizeKey(BuildKey(e)));
        }



        public void ProcessHeldKeys()
        {
            if (_heldKeys.Contains("W")) Camera?.MoveBackward();
            if (_heldKeys.Contains("S")) Camera?.MoveForward();
            if (_heldKeys.Contains("A")) Camera?.StrafeLeft();
            if (_heldKeys.Contains("D")) Camera?.StrafeRight();
        }

      
        private double lastMouseX = 0;
        private double lastMouseY = 0;


        public async Task HandleMouseDown(MouseEventArgs e)
        {
            if (e.Button == 2)
            {
                isRightMouseDown = true;
                lastMouseX = e.ClientX;
                lastMouseY = e.ClientY;
            }

            if (e.Button == 0)
            {
                isLeftMouseDown = true;
                _draggingLightIndex = -1;

                if (_engine != null)
                {
                    var lights = _engine.ActiveScene == 1
                        ? _engine.Scene.Lights
                        : _engine.Scene2.Lights;

                    for (int i = 0; i < lights.Count; i++)
                    {
                        var (sx, sy) = _engine.ProjectToScreen(lights[i].Position);
                        double dx = e.OffsetX - sx;
                        double dy = e.OffsetY - sy;
                        if (System.Math.Sqrt(dx * dx + dy * dy) < 25.0)
                        {
                            _draggingLightIndex = i;
                            lastMouseX = e.ClientX;
                            lastMouseY = e.ClientY;
                            break;
                        }
                    }
                }

                // Only paint if left click AND not grabbing a light
                if (_draggingLightIndex == -1 && _engine?.ActiveScene == 2)
                    _engine.HandleTileMapMouseDown((float)e.OffsetX, (float)e.OffsetY);
            }

            await Task.CompletedTask;
        }

        public async Task HandleMouseUp(MouseEventArgs e)
        {
           
            if (e.Button == 2) isRightMouseDown = false;
            if (e.Button == 0)
            {
                isLeftMouseDown = false;
                bool wasDraggingLight = _draggingLightIndex >= 0;
                _draggingLightIndex = -1;
                if (!wasDraggingLight && _engine?.ActiveScene == 2)
                    _engine.HandleTileMapMouseUp();
            }
          
            await Task.CompletedTask;
        }

        public async Task HandleMouseMove(MouseEventArgs e)
        {
            if (isRightMouseDown)
            {
                double deltaX = e.ClientX - lastMouseX;
                double deltaY = e.ClientY - lastMouseY;
                Camera?.Look((float)-deltaX, (float)-deltaY);
                lastMouseX = e.ClientX;
                lastMouseY = e.ClientY;
            }
            else if (isLeftMouseDown && _draggingLightIndex >= 0 && _engine != null)
            {
                var lights = _engine.ActiveScene == 1
                    ? _engine.Scene.Lights
                    : _engine.Scene2.Lights;

                if (_draggingLightIndex < lights.Count)
                {
                    var light = lights[_draggingLightIndex];
                    double deltaX = e.ClientX - lastMouseX;
                    double deltaY = e.ClientY - lastMouseY;
                    light.Position = new Vector3(
                        light.Position.X + (float)deltaX * 0.05f,
                        light.Position.Y + (_isShiftHeld ? (float)deltaY * -0.05f : 0f),
                        light.Position.Z - (!_isShiftHeld ? (float)deltaY * 0.05f : 0f)
                    );
                    lastMouseX = e.ClientX;
                    lastMouseY = e.ClientY;
                }
            }
            else if (isLeftMouseDown && _draggingLightIndex == -1 && _engine?.ActiveScene == 2)
            {
                _engine.HandleTileMapMouseMove((float)e.OffsetX, (float)e.OffsetY);
            }

            await Task.CompletedTask;
        }
        public Task PreventContextMenu(MouseEventArgs e)
        {
            // Prevents right-click context menu
            return Task.CompletedTask;
        }




// BREAK TO NEXT CODE--->>>>>


        private string BuildKey(KeyboardEventArgs e)
        {
            string key = e.Key switch
            {
                "Escape" => "ESCAPE",
                "ArrowUp" => "UP",
                "ArrowDown" => "DOWN",
                "ArrowLeft" => "LEFT",
                "ArrowRight" => "RIGHT",
                "w" => "W",
                "a" => "A",
                "s" => "S",
                "d" => "D",
                "3" => "3",
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
        public List<string> DebugMessages { get; } = new();

        /* ─────────────── DEFAULT BINDS ─────────────── */

        private void RegisterDefaultBinds()
        {
            Bind("ESCAPE", async () => await SpectralXGLX.ToggleViewport());

            // FPS-style camera movement
         //   Bind("W", async () => { Camera?.MoveForward(); await Task.CompletedTask; });
         //   Bind("S", async () => { Camera?.MoveBackward(); await Task.CompletedTask; });
         //   Bind("D", async () => { Camera?.StrafeLeft(); await Task.CompletedTask; });
         //   Bind("A", async () => { Camera?.StrafeRight(); await Task.CompletedTask; });


            //mouse binds should be in here-->>>

            Bind("3", async () => {
                ToggleDebugOverlay();            
                await Task.CompletedTask;
            });

            // ADD — press 4 to hide/show all UI panels
            Bind("4", async () => {
                SpectralXGLX.ToggleUIHidden();
                await Task.CompletedTask;
            });
            // Example combos
            BindSequence(new[] { "UP", "UP" }, async () => {
                DebugMessages.Add("Double UP!");
                await Task.CompletedTask;
            });

            BindSequence(new[] { "UP", "UP", "DOWN", "DOWN" }, async () => {
                DebugMessages.Add("Full Konami!");
                await Task.CompletedTask;
            }, timeout: TimeSpan.FromSeconds(3));

            BindSequence(new[] { "LEFT", "RIGHT" }, async () => {
                DebugMessages.Add("Left-Right combo!");
                await Task.CompletedTask;
            }, cooldown: TimeSpan.FromSeconds(1));

        }

        /* ─────────────── HELPERS ─────────────── */
        private class RecentKey
        {
            public string Key { get; set; }
            public DateTime Time { get; set; }
        }

        private class SequenceBind
        {
            public string[] Keys { get; set; }
            public Func<Task> Action { get; set; }
            public TimeSpan Timeout { get; set; }
            public DateTime LastTriggered { get; set; }
            public TimeSpan Cooldown { get; set; }
            public int Progress { get; set; } = 0;
        }
    }
}
