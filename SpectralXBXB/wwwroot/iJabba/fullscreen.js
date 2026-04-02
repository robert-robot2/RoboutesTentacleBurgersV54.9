


window.toggleFullscreen = function (elementId) {
    const elem = document.getElementById(elementId);
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


window.getViewportSize = (id) => {
    const el = document.getElementById(id);

    if (!el) return null;

    // If fullscreen, use actual screen size
    if (document.fullscreenElement) {
        return {
            width: window.innerWidth,
            height: window.innerHeight
        };
    }

    // Otherwise use element's real size
    const rect = el.getBoundingClientRect();

    return {
        width: Math.floor(rect.width),
        height: Math.floor(rect.height)
    };
}



window.registerFullscreenListener = function (dotnetRef) {
    document.addEventListener("fullscreenchange", () => {
        if (!document.fullscreenElement) {
            dotnetRef.invokeMethodAsync("OnFullscreenExit");
        }
    });
};

function spectralXLoop(dotnetRef) {
    function frame() {
        dotnetRef.invokeMethodAsync("Tick");
        requestAnimationFrame(frame);
    }
    requestAnimationFrame(frame);
}
