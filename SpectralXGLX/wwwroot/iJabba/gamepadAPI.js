// GamepadAPI.js - Polls gamepad state and sends to Blazor

window.GamepadAPI = {
    dotNetRef: null,
    isPolling: false,
    rafId: null,

    // Dead zone threshold (analog sticks)
    deadZone: 0.15,

    // Initialize and start polling
    init: function (dotNetReference) {
        console.log('[GamepadAPI] Initializing...');
        this.dotNetRef = dotNetReference;
        this.startPolling();
    },

    // Start polling loop
    startPolling: function () {
        if (this.isPolling) return;

        this.isPolling = true;
        console.log('[GamepadAPI] Polling started');

        const poll = () => {
            if (!this.isPolling) return;

            this.update();
            this.rafId = requestAnimationFrame(poll);
        };

        poll();
    },

    // Stop polling
    stopPolling: function () {
        this.isPolling = false;
        if (this.rafId) {
            cancelAnimationFrame(this.rafId);
            this.rafId = null;
        }
        console.log('[GamepadAPI] Polling stopped');
    },

    // Main update loop - called every frame
    update: function () {
        const gamepads = navigator.getGamepads();

        // Find first connected gamepad
        let gamepad = null;
        for (let i = 0; i < gamepads.length; i++) {
            if (gamepads[i]) {
                gamepad = gamepads[i];
                break;
            }
        }

        if (!gamepad) return;

        // Apply dead zone to axes
        const applyDeadZone = (value) => {
            return Math.abs(value) < this.deadZone ? 0 : value;
        };

        // Build state object
        const state = {
            // Left stick (movement)
            leftStickX: applyDeadZone(gamepad.axes[0] || 0),
            leftStickY: applyDeadZone(gamepad.axes[1] || 0),

            // Right stick (camera)
            rightStickX: applyDeadZone(gamepad.axes[2] || 0),
            rightStickY: applyDeadZone(gamepad.axes[3] || 0),

            // Buttons (standard Xbox/PlayStation layout)
            buttonA: gamepad.buttons[0]?.pressed || false,      // A/Cross
            buttonB: gamepad.buttons[1]?.pressed || false,      // B/Circle
            buttonX: gamepad.buttons[2]?.pressed || false,      // X/Square
            buttonY: gamepad.buttons[3]?.pressed || false,      // Y/Triangle

            leftBumper: gamepad.buttons[4]?.pressed || false,
            rightBumper: gamepad.buttons[5]?.pressed || false,

            leftTrigger: gamepad.buttons[6]?.value || 0,
            rightTrigger: gamepad.buttons[7]?.value || 0,

            buttonBack: gamepad.buttons[8]?.pressed || false,
            buttonStart: gamepad.buttons[9]?.pressed || false,

            leftStickButton: gamepad.buttons[10]?.pressed || false,
            rightStickButton: gamepad.buttons[11]?.pressed || false,

            dpadUp: gamepad.buttons[12]?.pressed || false,
            dpadDown: gamepad.buttons[13]?.pressed || false,
            dpadLeft: gamepad.buttons[14]?.pressed || false,
            dpadRight: gamepad.buttons[15]?.pressed || false
        };

        // Send to Blazor
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('UpdateGamepadState', state);
        }
    },

    // Cleanup
    dispose: function () {
        this.stopPolling();
        this.dotNetRef = null;
        console.log('[GamepadAPI] Disposed');
    }
};

// Listen for gamepad connect/disconnect
window.addEventListener('gamepadconnected', (e) => {
    console.log('[GamepadAPI] Gamepad connected:', e.gamepad.id);
});

window.addEventListener('gamepaddisconnected', (e) => {
    console.log('[GamepadAPI] Gamepad disconnected:', e.gamepad.id);
});