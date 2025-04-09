// ======================= SIDEBAR =========================
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

document.querySelectorAll('.sidebar-link').forEach(link => {
    link.addEventListener('click', function (event) {
        const sidebar = document.querySelector('.sidebar');
        if (!sidebar.classList.contains('collapsed')) return;

        event.preventDefault();
        const now = new Date().getTime();
        const lastClick = parseInt(this.getAttribute("data-click-time") || 0);

        if (now - lastClick < 300) {
            sidebar.classList.remove('collapsed');
        } else {
            this.setAttribute("data-click-time", now);
        }
    });
});

// ======================= PHÂN QUYỀN =========================
async function updateUserRoleAsync(checkbox) {
    const userId = checkbox.dataset.userid;
    const roleId = checkbox.dataset.roleid;

    // Nếu không phải là checked thì không xử lý
    if (!checkbox.checked) return;

    // Hủy chọn các checkbox còn lại của user
    document.querySelectorAll(`input[data-userid='${userId}']`).forEach(cb => {
        if (cb !== checkbox) cb.checked = false;
    });

    // Lấy token từ form ẩn
    const token = document.querySelector('#tokenForm input[name="__RequestVerificationToken"]')?.value;

    try {
        const response = await fetch('/Admin/UpdateUserRole', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                userId: parseInt(userId),
                newRoleId: parseInt(roleId)
            })
        });

        if (!response.ok) throw new Error("Lỗi máy chủ!");

        const result = await response.json();

        if (!result.success) {
            alert(result.message || "Có lỗi xảy ra khi cập nhật vai trò!");
            checkbox.checked = false;
        }
    } catch (err) {
        alert("Lỗi mạng hoặc hệ thống.");
        checkbox.checked = false;
        console.error(err);
    }
}
