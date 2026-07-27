window.themeInterop = {
    setTheme: function (theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('blazor-theme', theme);
    },
    getTheme: function () {
        return localStorage.getItem('blazor-theme') || 'dark';
    }
};