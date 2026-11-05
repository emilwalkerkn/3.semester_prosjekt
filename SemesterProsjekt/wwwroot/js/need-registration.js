(() => {
    const panel = document.getElementById("need-map-panel");
    const button = panel.querySelector(".need-fullscreen");
    const date = document.querySelector("[data-use-local-now]");
    if (date?.dataset.useLocalNow === "true") {
        const now = new Date();
        const localTime = new Date(now.getTime() - now.getTimezoneOffset() * 60000);
        date.value = localTime.toISOString().slice(0, 16);
    }
    button.addEventListener("click", async () => {
        try {
            if (document.fullscreenElement) await document.exitFullscreen();
            else await panel.requestFullscreen();
        } catch (_error) {
            button.textContent = "Fullskjerm er ikke tilgjengelig i denne nettleseren";
        }
    });
    document.addEventListener("fullscreenchange", () => {
        button.textContent = document.fullscreenElement === panel ? "Lukk fullskjerm" : "Vis kart i fullskjerm";
    });
})();
