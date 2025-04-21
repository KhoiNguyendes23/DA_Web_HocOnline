function initializeChat(senderId) {
    const connection = new signalR.HubConnectionBuilder().withUrl("/chathub").build();
    let currentReceiverId = null;

    // Kết nối đến Hub
    connection.start()
        .then(() => console.log("✅ Kết nối SignalR thành công"))
        .catch(err => console.error("❌ Kết nối thất bại:", err));

    // Gửi tin nhắn khi click Gửi
    document.getElementById("chat-send").addEventListener("click", sendMessage);

    // Gửi khi nhấn Enter
    document.getElementById("chat-input").addEventListener("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            sendMessage();
        }
    });

    // Khi click người dùng
    document.querySelectorAll(".user-item").forEach(item => {
        item.addEventListener("click", function () {
            currentReceiverId = parseInt(this.dataset.userid);
            const fullName = this.querySelector("strong").innerText;
            document.querySelector("#chat-title span").innerText = fullName;

            // Bôi đậm người đang chat
            document.querySelectorAll(".user-item").forEach(i => i.classList.remove("active"));
            this.classList.add("active");

            // Load lịch sử chat
            const container = document.getElementById("chat-messages");
            container.innerHTML = "<i>Đang tải tin nhắn...</i>";

            fetch(`/api/chat/messages?senderId=${senderId}&receiverId=${currentReceiverId}`)
                .then(res => res.json())
                .then(messages => {
                    container.innerHTML = "";
                    messages.forEach(m => {
                        const isMine = m.senderId === senderId;
                        appendMessage(isMine ? "Bạn" : "Họ", m.message, isMine);
                    });
                })
                .catch(err => {
                    container.innerHTML = "<span class='text-danger'>Không tải được tin nhắn!</span>";
                    console.error("❌ Lỗi khi tải tin nhắn:", err);
                });
        });
    });

    // Gửi tin nhắn
    function sendMessage() {
        const input = document.getElementById("chat-input");
        const message = input.value.trim();

        if (!message) {
            console.warn("❗ Chưa nhập nội dung tin nhắn");
            return;
        }

        if (!currentReceiverId) {
            console.warn("❗ Chưa chọn người nhận");
            return;
        }

        connection.invoke("SendMessage", senderId, currentReceiverId, message)
            .then(() => {
                console.log("✅ Tin nhắn đã gửi:", message);
                appendMessage("Bạn", message, true);
                input.value = "";
            })
            .catch(err => {
                console.error("❌ Lỗi khi gửi tin nhắn:", err.toString());
            });
    }

    // Nhận tin nhắn realtime
    connection.on("ReceiveMessage", function (fromId, message) {
        if (parseInt(fromId) === currentReceiverId) {
            appendMessage("Họ", message, false);
        } else {
            console.log("📩 Nhận tin nhắn từ user khác (không active):", fromId, message);
        }
    });

    // Thêm tin nhắn vào khung
    function appendMessage(senderLabel, message, isMine) {
        const container = document.getElementById("chat-messages");

        const wrapper = document.createElement("div");
        wrapper.className = `d-flex justify-content-${isMine ? "end" : "start"} align-items-end mb-2`;

        const avatar = document.createElement("img");
        avatar.src = isMine
            ? "/images/Avatar_images/default-avatar.png"  // Avatar của chính mình
            : document.querySelector(".user-item.active img")?.src || "/images/Avatar_images/default-avatar.png"; // Avatar đối phương

        avatar.className = "chat-avatar";
        avatar.width = 32;
        avatar.height = 32;

        const div = document.createElement("div");
        div.className = isMine ? "message message-sent" : "message message-received";
        div.innerText = message;

        if (isMine) {
            // Bạn: hiển thị bubble trước, avatar sau
            wrapper.appendChild(div);
            wrapper.appendChild(avatar);
        } else {
            // Họ: avatar trước, bubble sau
            wrapper.appendChild(avatar);
            wrapper.appendChild(div);
        }

        container.appendChild(wrapper);
        container.scrollTop = container.scrollHeight;
    }



    // Escape XSS
    function escapeHtml(text) {
        const div = document.createElement("div");
        div.innerText = text;
        return div.innerHTML;
    }
}
