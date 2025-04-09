// wwwroot/js/decentralization.js
// Gửi bất đồng bộ khi đổi checkbox
async function updateUserRole(userId, newRoleId, checkbox) {
    const isChecked = checkbox.checked;

    // Nếu bỏ chọn checkbox và đó là vai trò hiện tại => không làm gì cả
    if (!isChecked) return;

    const formData = new FormData();
    formData.append("userId", userId);
    formData.append("newRoleId", newRoleId);

    try {
        const response = await fetch("/Admin/UpdateUserRole", {
            method: "POST",
            body: formData
        });

        const data = await response.json();

        if (!response.ok || data.success === false) {
            alert(data.message || "Đã xảy ra lỗi khi cập nhật vai trò!");
            checkbox.checked = false;
        } else {
            // Cập nhật lại tất cả checkbox của hàng đang được chọn
            const row = checkbox.closest("tr");
            row.querySelectorAll("input[type='checkbox']").forEach(cb => {
                cb.checked = parseInt(cb.dataset.role) === newRoleId;
            });
        }
    } catch (error) {
        alert("Lỗi mạng hoặc máy chủ không phản hồi.");
        console.error(error);
        checkbox.checked = false;
    }
}
function handleRoleChange(changedCheckbox) {
    const userId = changedCheckbox.dataset.userId;
    const selectedRoleId = changedCheckbox.dataset.roleId;

    // Bỏ chọn các checkbox khác của cùng người dùng
    document.querySelectorAll(`.role-checkbox[data-user-id='${userId}']`).forEach(cb => {
        if (cb !== changedCheckbox) cb.checked = false;
    });

    // Gửi bất đồng bộ (AJAX)
    fetch('/Admin/UpdateUserRole', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({
            userId: parseInt(userId),
            newRoleId: parseInt(selectedRoleId)
        })
    })
        .then(response => {
            if (!response.ok) throw new Error("Lỗi khi cập nhật!");
        })
        .catch(error => {
            alert("❌ Có lỗi xảy ra: " + error.message);
            // Nếu lỗi, checkbox sẽ được khôi phục lại sau 1s
            setTimeout(() => window.location.reload(), 1000);
        });
}


