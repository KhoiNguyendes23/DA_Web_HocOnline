(() => {
    const openNav = document.querySelector('.open-menu'),
        closeNav = document.querySelector('.close-menu'),
        navMenu = document.querySelector('.nav-links-container'),
        background = document.querySelector('.background'),
        mediaSize = 992;

    openNav.addEventListener('click', toggleMenu);
    closeNav.addEventListener('click', toggleMenu);
    background.addEventListener('click', toggleMenu);

    function toggleMenu() {
        navMenu.classList.toggle("open");
        background.classList.toggle("active");
    }

    navMenu.addEventListener("click", (event) => {
        if (event.target.hasAttribute("data-toggle") && window.innerWidth <= mediaSize) {
            event.preventDefault();
            const dropdownMenuBranch = event.target.parentElement;

            if (dropdownMenuBranch.classList.contains("active")) {
                collapseDropdownMenu();
            } else {
                if (navMenu.querySelector(".dropdown-menu-branch.active")) {
                    collapseDropdownMenu();
                }

                dropdownMenuBranch.classList.add("active");
                const dropdownMenu = dropdownMenuBranch.querySelector(".custom-dropdown-menu");
                dropdownMenu.style.maxHeight = dropdownMenu.scrollHeight + "px";
            }
        }
    });

    // Xử lý menu cấp 2 cho mobile
    navMenu.addEventListener("click", (event) => {
        if (event.target.closest(".has-submenu > a") && window.innerWidth <= mediaSize) {
            event.preventDefault();
            const parentItem = event.target.closest(".has-submenu");

            // Toggle menu cấp 2
            const submenu = parentItem.querySelector(".sub-dropdown-menu");

            if (parentItem.classList.contains("open-submenu")) {
                submenu.style.maxHeight = null;
                parentItem.classList.remove("open-submenu");
            } else {
                // Ẩn tất cả menu cấp 2 khác
                navMenu.querySelectorAll(".has-submenu").forEach(item => {
                    item.classList.remove("open-submenu");
                    const sub = item.querySelector(".sub-dropdown-menu");
                    if (sub) sub.style.maxHeight = null;
                });

                submenu.style.maxHeight = submenu.scrollHeight + "px";
                parentItem.classList.add("open-submenu");
            }
        }
    });


    function collapseDropdownMenu() {
        navMenu.querySelector(".dropdown-menu-branch.active .custom-dropdown-menu").removeAttribute("style");
        navMenu.querySelector(".dropdown-menu-branch.active").classList.remove("active");
    }

})();
