// fullscreen.js

window.toggleFullscreen = function (elementId) {
    const elem = document.getElementById(elementId);
    if (!elem) return;

    if (!document.fullscreenElement) {
        if (elem.requestFullscreen) {
            elem.requestFullscreen();
        } else if (elem.webkitRequestFullscreen) { // Safari
            elem.webkitRequestFullscreen();
        } else if (elem.msRequestFullscreen) { // IE11
            elem.msRequestFullscreen();
        }
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        }
    }
};

window.getViewportSize = function (id) {
    const el = document.getElementById(id);
    if (!el) return null;

    // If fullscreen, use window size
    if (document.fullscreenElement) {
        return {
            width: window.innerWidth,
            height: window.innerHeight
        };
    }

    const rect = el.getBoundingClientRect();
    return {
        width: Math.floor(rect.width),
        height: Math.floor(rect.height)
    };
};

// Registers a fullscreen listener that notifies .NET on exit or change
window.registerFullscreenListener = function (dotnetRef, elementId) {
    const handler = () => {
        const size = window.getViewportSize(elementId);
        if (!size) return;

        // Notify .NET about new size after fullscreen change
        dotnetRef.invokeMethodAsync("OnCanvasResized", size.width, size.height);

        if (!document.fullscreenElement) {
            // Optional: dedicated exit callback if you keep it on the C# side
            if (dotnetRef._onFullscreenExitInvoked !== true) {
                // You can add a separate OnFullscreenExit if you want
                dotnetRef._onFullscreenExitInvoked = true;
            }
        } else {
            dotnetRef._onFullscreenExitInvoked = false;
        }
    };

    document.addEventListener("fullscreenchange", handler);
};
