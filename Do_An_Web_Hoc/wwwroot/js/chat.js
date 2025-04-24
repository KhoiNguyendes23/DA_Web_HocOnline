function initializeChat(senderId) {
    const connection = new signalR.HubConnectionBuilder().withUrl("/chathub").build();
    let currentReceiverId = null;
    let selectedImageFile = null;

    connection.start()
        .then(() => console.log("✅ SignalR connected"))
        .catch(err => console.error("❌ SignalR connection failed:", err));

    // Chọn ảnh
    document.getElementById("chat-image-btn").addEventListener("click", () => {
        document.getElementById("chat-image").click();
    });

    document.getElementById("chat-image").addEventListener("change", function () {
        selectedImageFile = this.files[0] || null;
        if (selectedImageFile) showImagePreview(selectedImageFile);
        this.value = "";
    });

    document.getElementById("chat-send").addEventListener("click", sendMessage);
    document.getElementById("chat-input").addEventListener("keydown", e => {
        if (e.key === "Enter") {
            e.preventDefault();
            sendMessage();
        }
    });

    async function sendMessage() {
        const input = document.getElementById("chat-input");
        const message = input.value.trim();
        if (!message && !selectedImageFile) return;
        if (!currentReceiverId) return alert("❗ Vui lòng chọn người nhận");

        let imageUrl = null;
        if (selectedImageFile) {
            const formData = new FormData();
            formData.append("imageFile", selectedImageFile);
            try {
                const res = await fetch("/api/chat/upload", {
                    method: "POST",
                    body: formData
                });
                const data = await res.json();
                imageUrl = data.imageUrl;
            } catch (err) {
                alert("❌ Upload ảnh thất bại");
                return;
            }
        }

        await connection.invoke("SendMessageFull", senderId, currentReceiverId, message, imageUrl);

        if (message) appendMessage("Bạn", message, true);
        if (imageUrl) appendImage(true, imageUrl);

        input.value = "";
        selectedImageFile = null;
        document.getElementById("image-preview-container").innerHTML = "";
    }

    document.querySelectorAll(".user-item").forEach(item => {
        item.addEventListener("click", function () {
            currentReceiverId = parseInt(this.dataset.userid);
            document.querySelector("#chat-title span").innerText = this.querySelector("strong").innerText;

            document.querySelectorAll(".user-item").forEach(i => i.classList.remove("active"));
            this.classList.add("active");

            const container = document.getElementById("chat-messages");
            container.innerHTML = "<i>Đang tải...</i>";

            fetch(`/api/chat/messages?senderId=${senderId}&receiverId=${currentReceiverId}`)
                .then(res => res.json())
                .then(messages => {
                    container.innerHTML = "";
                    messages.forEach(m => {
                        const isMine = m.senderId === senderId;
                        if (m.imageUrl) appendImage(isMine, m.imageUrl);
                        if (m.message) appendMessage(isMine ? "Bạn" : "Họ", m.message, isMine);
                    });

                    return fetch("/api/chat/mark-as-read", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({
                            senderId: currentReceiverId,
                            receiverId: senderId
                        })
                    });
                })
                .then(() => console.log("✅ Tin nhắn đã được đánh dấu là đã đọc."))
                .catch(err => {
                    container.innerHTML = "<span class='text-danger'>Không thể tải tin nhắn</span>";
                    console.error("❌ Lỗi tải hoặc mark-as-read:", err);
                });
        });
    });

    connection.on("ReceiveMessageFull", (fromId, message, imageUrl) => {
        if (parseInt(fromId) === currentReceiverId) {
            if (imageUrl) appendImage(false, imageUrl);
            if (message) appendMessage("Họ", message, false);
        } else {
            console.log("📩 Tin nhắn từ người khác:", fromId);
        }
    });

    function showImagePreview(file) {
        const preview = document.getElementById("image-preview-container");
        preview.innerHTML = "";
        const img = document.createElement("img");
        img.src = URL.createObjectURL(file);
        img.className = "chat-image";
        img.style.maxWidth = "200px";
        img.style.borderRadius = "8px";
        img.style.marginBottom = "10px";
        preview.appendChild(img);
    }

    function appendMessage(label, message, isMine) {
        const container = document.getElementById("chat-messages");
        const wrapper = document.createElement("div");
        wrapper.className = `d-flex justify-content-${isMine ? "end" : "start"} align-items-end mb-2`;

        const avatar = document.createElement("img");
        avatar.src = isMine ? "/images/Avatar_images/default-avatar.png"
            : document.querySelector(".user-item.active img")?.src || "/images/Avatar_images/default-avatar.png";
        avatar.className = "chat-avatar";
        avatar.width = 32;
        avatar.height = 32;

        const div = document.createElement("div");
        div.className = isMine ? "message message-sent" : "message message-received";
        div.innerText = message;

        if (isMine) {
            wrapper.appendChild(div);
            wrapper.appendChild(avatar);
        } else {
            wrapper.appendChild(avatar);
            wrapper.appendChild(div);
        }

        container.appendChild(wrapper);
        container.scrollTop = container.scrollHeight;
    }

    function appendImage(isMine, imageUrl) {
        const container = document.getElementById("chat-messages");
        const wrapper = document.createElement("div");
        wrapper.className = `d-flex justify-content-${isMine ? "end" : "start"} align-items-end mb-2`;

        const avatar = document.createElement("img");
        avatar.src = isMine ? "/images/Avatar_images/default-avatar.png"
            : document.querySelector(".user-item.active img")?.src || "/images/Avatar_images/default-avatar.png";
        avatar.className = "chat-avatar";
        avatar.width = 32;
        avatar.height = 32;

        const img = document.createElement("img");
        img.src = imageUrl;
        img.className = "chat-image";
        img.style.maxWidth = "200px";
        img.style.borderRadius = "8px";
        img.style.boxShadow = "0 2px 6px rgba(0,0,0,0.1)";
        img.style.cursor = "pointer";

        const imgWrapper = document.createElement("div");
        imgWrapper.className = isMine ? "message message-sent" : "message message-received";
        imgWrapper.appendChild(img);

        if (isMine) {
            wrapper.appendChild(imgWrapper);
            wrapper.appendChild(avatar);
        } else {
            wrapper.appendChild(avatar);
            wrapper.appendChild(imgWrapper);
        }

        container.appendChild(wrapper);
        container.scrollTop = container.scrollHeight;
    }
}
