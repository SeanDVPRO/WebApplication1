// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Enhanced Responsive Sidebar Navigation
(function () {
    document.addEventListener("DOMContentLoaded", function () {
        const toggleBtn = document.getElementById("sidebarToggle");
        const sidebar = document.querySelector(".sidebar");
        const body = document.body;
        const overlay = document.querySelector('.sidebar-overlay');

        if (!toggleBtn || !sidebar) {
            console.warn("Sidebar elements not found");
            return;
        }

        const isMobile = () => window.matchMedia('(max-width: 768px)').matches;

        const savedCollapsed = localStorage.getItem("sidebarCollapsed") === "true";
        if (!isMobile() && savedCollapsed) {
            sidebar.classList.add("collapsed");
            body.classList.add("sidebar-collapsed");
            toggleBtn.setAttribute('aria-expanded', 'false');
            toggleBtn.setAttribute('aria-label', 'Expand sidebar');
        }

        function openMobileSidebar() {
            sidebar.classList.add('mobile-open');
            overlay && overlay.classList.add('active');
            toggleBtn.setAttribute('aria-label', 'Close sidebar');
            toggleBtn.setAttribute('aria-expanded', 'true');
            const firstLink = sidebar.querySelector('.nav-link');
            if (firstLink) firstLink.focus();
        }

        function closeMobileSidebar() {
            sidebar.classList.remove('mobile-open');
            overlay && overlay.classList.remove('active');
            toggleBtn.setAttribute('aria-label', 'Open sidebar');
            toggleBtn.setAttribute('aria-expanded', 'false');
        }

        function toggleDesktopSidebar() {
            const collapsed = sidebar.classList.toggle("collapsed");
            body.classList.toggle("sidebar-collapsed", collapsed);
            localStorage.setItem("sidebarCollapsed", collapsed.toString());
            toggleBtn.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
            toggleBtn.setAttribute('aria-label', collapsed ? 'Expand sidebar' : 'Collapse sidebar');
        }

        toggleBtn.addEventListener('click', function () {
            if (isMobile()) {
                if (sidebar.classList.contains('mobile-open')) {
                    closeMobileSidebar();
                } else {
                    openMobileSidebar();
                }
            } else {
                toggleDesktopSidebar();
            }
        });

        overlay && overlay.addEventListener('click', closeMobileSidebar);

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && isMobile() && sidebar.classList.contains('mobile-open')) {
                closeMobileSidebar();
            }
        });

        let resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                if (isMobile()) {
                    body.classList.remove('sidebar-collapsed');
                    sidebar.classList.remove('collapsed');
                    closeMobileSidebar();
                } else {
                    const persistedCollapsed = localStorage.getItem("sidebarCollapsed") === "true";
                    sidebar.classList.toggle('collapsed', persistedCollapsed);
                    body.classList.toggle('sidebar-collapsed', persistedCollapsed);
                    toggleBtn.setAttribute('aria-expanded', persistedCollapsed ? 'false' : 'true');
                    toggleBtn.setAttribute('aria-label', persistedCollapsed ? 'Expand sidebar' : 'Collapse sidebar');
                    overlay && overlay.classList.remove('active');
                }
            }, 150);
        });

        const navLinks = sidebar.querySelectorAll('.nav-link[href]');
        const currentPath = window.location.pathname.toLowerCase();
        navLinks.forEach(link => {
            try {
                const href = new URL(link.href).pathname.toLowerCase();
                if (href === currentPath) {
                    link.classList.add('active');
                }
            } catch (_) { /* ignore */ }
        });
    });
})();