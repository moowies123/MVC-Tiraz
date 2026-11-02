(function () {
    var root = document.documentElement, KEY = "tiraz-theme";
    function get() { try { return localStorage.getItem(KEY); } catch (e) { return null; } }
    function set(v) { try { localStorage.setItem(KEY, v); } catch (e) { } }

    // the dark styles live in this file, so nothing else has to be copied
    var style = document.createElement("style");
    style.textContent = `/* Dark mode: flips the colour variables used by home.css */
html[data-theme="dark"] {
	--color-scheme: dark;
	--black: #f2f2f2;
	--white: #0d0d0d;
	--soft: #1c1c1c;
	color-scheme: dark;
}
html[data-theme="dark"] body { background: #0d0d0d; color: #f2f2f2; }
html[data-theme="dark"] .search input::placeholder { color: #9a9a9a; }
html[data-theme="dark"] .heart-btn svg path { fill: #f2f2f2; stroke: #f2f2f2; }
html[data-theme="dark"] .heart-btn:hover { background: #f2f2f2; }
html[data-theme="dark"] .heart-btn:hover svg path { fill: #0d0d0d; stroke: #0d0d0d; }

/* buttons that were hard-coded black/white */
html[data-theme="dark"] .btn-wish { color: #f2f2f2; border-color: #f2f2f2; }
html[data-theme="dark"] .btn-wish:hover { background: #f2f2f2; color: #0d0d0d; }

/* footer */
html[data-theme="dark"] .site-footer { background: #141414; color: #f2f2f2; border-top-color: #2a2a2a; }
html[data-theme="dark"] .footer-copy { border-top-color: #2a2a2a; }
html[data-theme="dark"] .social-icons a { color: #f2f2f2; border-color: #f2f2f2; }
html[data-theme="dark"] .social-icons a:hover { background: #f2f2f2; color: #0d0d0d; }

/* wishlist page */
html[data-theme="dark"] .wl-card, html[data-theme="dark"] .wl-empty { background: #1c1c1c; }
html[data-theme="dark"] .wl-photo { background: #2a2a2a; }
html[data-theme="dark"] .wl-btn { background: #f2f2f2; color: #0d0d0d; border-color: #f2f2f2; }
html[data-theme="dark"] .wl-btn:hover { background: transparent; color: #f2f2f2; }
html[data-theme="dark"] .wl-btn-outline { background: transparent; color: #f2f2f2; }
html[data-theme="dark"] .wl-btn-outline:hover { background: #f2f2f2; color: #0d0d0d; }

.theme-btn svg { width: 22px; height: 22px; fill: none; stroke: currentColor; stroke-width: 1.8; stroke-linecap: round; stroke-linejoin: round; }
`;
    document.head.appendChild(style);

    // apply immediately (before the page paints) so there is no white flash
    var saved = get() || (window.matchMedia && matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light");
    root.setAttribute("data-theme", saved);

    var MOON = '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M20 14.5A8 8 0 0 1 9.5 4 8 8 0 1 0 20 14.5z"/></svg>';
    var SUN = '<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="4"/><path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4"/></svg>';

    document.addEventListener("DOMContentLoaded", function () {
        var bar = document.querySelector(".topbar-right");
        if (!bar) return;
        var btn = document.createElement("button");
        btn.type = "button";
        btn.className = "icon-btn theme-btn";
        btn.setAttribute("aria-label", "Toggle dark mode");
        function paint() { btn.innerHTML = root.getAttribute("data-theme") === "dark" ? SUN : MOON; }
        btn.addEventListener("click", function () {
            var next = root.getAttribute("data-theme") === "dark" ? "light" : "dark";
            root.setAttribute("data-theme", next);
            set(next);
            paint();
        });
        paint();
        bar.insertBefore(btn, bar.firstChild);
    });
})();
