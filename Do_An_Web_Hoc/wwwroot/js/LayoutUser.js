
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

//-----DANH MỤC KHÓA HỌC-----
document.querySelector('.submenu-link[data-view="registered"]').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/RegisteredCourses";
});

document.querySelector('.submenu-link[data-view="completed"]').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/Completed";
});
//-----KHÓA HỌC CỦA TÔI-----
document.querySelector('.submenu-link[data-view="FreeCourses"]').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/FreeCourses";
});

document.querySelector('.submenu-link[data-view="PaidCourses"]').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/PaidCourses";
});
//-----LUYỆN TẬP-------
document.querySelector('.submenu-link[data-view="Quiz"]').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/Quiz";
});

document.querySelector('.submenu-link[data-view="Practice"]').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/Practice";
});

document.querySelector('.submenu-link[data-view="Ranking"]').addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/Ranking";
});

//-----HỖ TRỢ-----
document.querySelector('.submenu-link[data-view="Support"]')?.addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/Support";
});
document.querySelector('.submenu-link[data-view="ContactLecturer"]')?.addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/ContactLecturer";
});
document.querySelector('.submenu-link[data-view="Community"]')?.addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/Community";
});
//-----TÀI KHOẢN CỦA TÔI-----
document.querySelector('.submenu-link[data-view="Profile"]')?.addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/Profile";
});

document.querySelector('.submenu-link[data-view="PaymentHistory"]')?.addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/PaymentHistory";
});

document.querySelector('.submenu-link[data-view="SecuritySettings"]')?.addEventListener('click', function (e) {
    e.preventDefault();
    window.location.href = "/User/SecuritySettings";
});









