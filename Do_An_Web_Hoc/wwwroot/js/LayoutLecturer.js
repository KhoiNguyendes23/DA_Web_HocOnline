
function toggleMenu(menuId) {
    const submenu = document.getElementById(menuId);
    const icon = submenu.previousElementSibling.querySelector('.dropdown-icon');

    const isOpen = submenu.classList.contains("open");
    submenu.classList.toggle("open");
    submenu.style.display = isOpen ? "none" : "block";
    icon.style.transform = isOpen ? "rotate(0deg)" : "rotate(180deg)";
}

function toggleSidebar() {
    const sidebar = document.querySelector('.sidebar');
    sidebar.classList.toggle('collapsed');

    if (sidebar.classList.contains('collapsed')) {
        document.querySelectorAll('.submenu').forEach(menu => {
            menu.classList.remove("open");
            menu.style.display = "none";
        });
    }
}

// Double click mở rộng lại sidebar khi đang thu gọn
document.querySelectorAll('.sidebar-link').forEach(link => {
    link.addEventListener('click', function (event) {
        const sidebar = document.querySelector('.sidebar');
        if (!sidebar.classList.contains('collapsed')) return;

        event.preventDefault(); // Ngăn không cho submenu mở ra khi sidebar bị thu gọn
        const now = new Date().getTime();
        const lastClick = parseInt(this.getAttribute("data-click-time") || 0);

        if (now - lastClick < 300) {
            sidebar.classList.remove('collapsed');
        } else {
            this.setAttribute("data-click-time", now);
        }
    });
});


