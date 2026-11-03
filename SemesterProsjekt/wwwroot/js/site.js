// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Nattmodus gjelder for hele portalen og huskes lokalt i brukerens nettleser.
(() => {
    const storageKey = "kriseportal-theme";
    const root = document.documentElement;
    const toggle = document.getElementById("darkModeToggle");

    if (!toggle) {
        return;
    }

    const updateToggle = () => {
        const isDarkMode = root.classList.contains("dark-mode");
        toggle.setAttribute("aria-checked", isDarkMode.toString());
        toggle.setAttribute("aria-label", isDarkMode ? "Slå av nattmodus" : "Slå på nattmodus");
    };

    updateToggle();
    toggle.addEventListener("click", () => {
        const isDarkMode = root.classList.toggle("dark-mode");
        localStorage.setItem(storageKey, isDarkMode ? "dark" : "light");
        updateToggle();
    });
})();
