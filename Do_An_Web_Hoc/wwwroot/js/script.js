
document.addEventListener("DOMContentLoaded", function () {
    // Toggle sidebar
    const toggleBtn = document.querySelector(".toggle-btn");
    const sidebar = document.querySelector(".sidebar");

    toggleBtn.addEventListener("click", function () {
        sidebar.classList.toggle("collapsed");

        if (sidebar.classList.contains("collapsed")) {
            document.querySelectorAll(".submenu").forEach(menu => {
                menu.classList.remove("open");
                menu.style.display = "none";
            });
        }
    });

    // Toggle submenu
    document.querySelectorAll(".sidebar-link").forEach(link => {
        link.addEventListener("click", function (event) {
            const sidebar = document.querySelector(".sidebar");
            const menuId = this.getAttribute("onclick")?.match(/'(.*?)'/)?.[1];
            const submenu = document.getElementById(menuId);

            if (!submenu) return;

            if (sidebar.classList.contains("collapsed")) {
                event.preventDefault();
                let clickTime = parseInt(this.getAttribute("data-click-time") || 0);
                let now = new Date().getTime();

                if (clickTime && now - clickTime < 300) {
                    sidebar.classList.remove("collapsed");
                } else {
                    this.setAttribute("data-click-time", now);
                }
                return;
            }

            if (submenu.classList.contains("open")) {
                submenu.classList.remove("open");
                submenu.style.display = "none";
            } else {
                // M? menu hi?n t?i (KHÔNG ðóng các menu khác)
                submenu.classList.add("open");
                submenu.style.display = "block";
            }

        });
    });
});
