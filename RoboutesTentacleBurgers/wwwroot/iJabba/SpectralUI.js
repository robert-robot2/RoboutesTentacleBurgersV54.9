
window.toggleFullscreen = function (elementId) {
    const elem = document.getElementById(elementId);
    if (!elem) return;
    if (!document.fullscreenElement) {
        if (elem.requestFullscreen) elem.requestFullscreen();
        else if (elem.webkitRequestFullscreen) elem.webkitRequestFullscreen();
    } else {
        if (document.exitFullscreen) document.exitFullscreen();
    }
};

window.getViewportSize = function (id) {
    const el = document.getElementById(id);
    if (!el) return null;
    if (document.fullscreenElement) return { width: window.innerWidth, height: window.innerHeight };
    const rect = el.getBoundingClientRect();
    return { width: Math.floor(rect.width), height: Math.floor(rect.height) };
};

window.registerFullscreenListener = function (dotnetRef, elementId) {
    document.addEventListener("fullscreenchange", () => {
        const size = window.getViewportSize(elementId);
        if (size) dotnetRef.invokeMethodAsync("OnCanvasResized", size.width, size.height);
    });
};

window.GamepadAPI = {
    dotNetRef: null, isPolling: false, rafId: null, deadZone: 0.15,
    init: function (ref) { this.dotNetRef = ref; this.startPolling(); },
    startPolling: function () {
        if (this.isPolling) return;
        this.isPolling = true;
        const poll = () => { if (!this.isPolling) return; this.update(); this.rafId = requestAnimationFrame(poll); };
        poll();
    },
    stopPolling: function () {
        this.isPolling = false;
        if (this.rafId) { cancelAnimationFrame(this.rafId); this.rafId = null; }
    },
    update: function () {
        const gamepads = navigator.getGamepads();
        let gp = null;
        for (let i = 0; i < gamepads.length; i++) { if (gamepads[i]) { gp = gamepads[i]; break; } }
        if (!gp) return;
        const dz = (v) => Math.abs(v) < this.deadZone ? 0 : v;
        const state = {
            leftStickX: -dz(gp.axes[0] || 0), leftStickY: -dz(gp.axes[1] || 0),
            rightStickX: -dz(gp.axes[2] || 0), rightStickY: -dz(gp.axes[3] || 0),
            buttonA: gp.buttons[0]?.pressed || false, buttonB: gp.buttons[1]?.pressed || false,
            buttonX: gp.buttons[2]?.pressed || false, buttonY: gp.buttons[3]?.pressed || false,
            leftBumper: gp.buttons[4]?.pressed || false, rightBumper: gp.buttons[5]?.pressed || false,
            leftTrigger: gp.buttons[6]?.value || 0, rightTrigger: gp.buttons[7]?.value || 0,
            buttonBack: gp.buttons[8]?.pressed || false, buttonStart: gp.buttons[9]?.pressed || false,
            leftStickButton: gp.buttons[10]?.pressed || false, rightStickButton: gp.buttons[11]?.pressed || false,
            dpadUp: gp.buttons[12]?.pressed || false, dpadDown: gp.buttons[13]?.pressed || false,
            dpadLeft: gp.buttons[14]?.pressed || false, dpadRight: gp.buttons[15]?.pressed || false
        };
        if (this.dotNetRef) this.dotNetRef.invokeMethodAsync('UpdateGamepadState', state);
    },
    dispose: function () { this.stopPolling(); this.dotNetRef = null; }
};

window.addEventListener('gamepadconnected', (e) => { console.log('[GamepadAPI] connected:', e.gamepad.id); });
window.addEventListener('gamepaddisconnected', (e) => { console.log('[GamepadAPI] disconnected:', e.gamepad.id); });





window.SpectralGLLoader = {
    _requested: 0,
    _completed: 0,
    _visible: false,
    _specialFlags: { tilemap: false, cubemap: false },
    _needsSpecial: { tilemap: false, cubemap: false },

    show: function () {
        const el = document.getElementById('SpectralX-Loader');
        console.log('[Loader] show() — el found:', !!el);
        if (el) { el.style.display = 'flex'; this._visible = true; }
    },

    hide: function () {
        const el = document.getElementById('SpectralX-Loader');
        if (el) { el.style.display = 'none'; this._visible = false; }
    },

    reset: function (needsTilemap, needsCubemap) {
        this._requested = 0;
        this._completed = 0;
        this._specialFlags.tilemap = false;
        this._specialFlags.cubemap = false;
        this._needsSpecial.tilemap = needsTilemap || false;
        this._needsSpecial.cubemap = needsCubemap || false;
        this.updateDisplay(0);

        this.show();
        // At the end of reset(), after this.show():
        if (window.SpectralGLInterop?.resetParticles)
            window.SpectralGLInterop.resetParticles();
    },

    onAssetRequested: function () {
        this._requested++;
    },

    onAssetComplete: function () {
        this._completed++;
        const pct = this.getPercentage();
        this.updateDisplay(pct);
        if (pct >= 100) this.hide();
    },
    onSpecialComplete: function (flag) {
        this._specialFlags[flag] = true;
        this.updateDisplay(this.getPercentage());
    },

    getPercentage: function () {
        if (this._requested === 0) return 100;
        let basePercent = (this._completed / this._requested) * 100;

        // Special flags each reserve a slice of the last 10%
        let specialCount = 0;
        let specialDone = 0;
        if (this._needsSpecial.tilemap) { specialCount++; if (this._specialFlags.tilemap) specialDone++; }
        if (this._needsSpecial.cubemap) { specialCount++; if (this._specialFlags.cubemap) specialDone++; }

        if (specialCount > 0) {
            basePercent = Math.min(basePercent, 90);
            basePercent += (specialDone / specialCount) * 10;
        }

        return Math.min(Math.floor(basePercent), 100);
    },

    isDone: function () {
        if (!this._visible) return false;
        if (this._completed < this._requested) return false;
        if (this._needsSpecial.tilemap && !this._specialFlags.tilemap) return false;
        if (this._needsSpecial.cubemap && !this._specialFlags.cubemap) return false;
        return true;
    },

    updateDisplay: function (pct) {
        const text = document.getElementById('SpectralX-Loader');
        if (!text) return;
        const textEl = text.querySelector('.sx-loading-text');
        const circle = text.querySelector('.sx-loading-progress circle:last-child');
        if (textEl) textEl.textContent = pct + '%';
        if (circle) {
            const circumference = 251.2;
            const fill = (pct / 100) * circumference;
            circle.style.strokeDasharray = fill + ',' + circumference;
        }
    }
};

