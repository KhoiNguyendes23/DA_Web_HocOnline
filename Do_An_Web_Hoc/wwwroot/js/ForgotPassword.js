document.addEventListener("DOMContentLoaded", function () {
    const step1 = document.getElementById("step1");
    const step2 = document.getElementById("step2");
    const step3 = document.getElementById("step3");

    const sendOtpBtn = document.getElementById("sendOtpBtn");
    const verifyOtpBtn = document.getElementById("verifyOtpBtn");
    const resetPasswordForm = document.getElementById("resetPasswordForm");

    const emailInput = document.getElementById("emailInput");
    const otpHiddenInput = document.getElementById("otpHiddenInput"); // Input ẩn để chứa OTP
    const newPasswordInput = document.getElementById("newPassword");
    const confirmPasswordInput = document.getElementById("confirmPassword");

    // ✅ Xử lý gửi mã OTP
    sendOtpBtn.addEventListener("click", async function (e) {
        e.preventDefault();
        const email = emailInput.value.trim();

        if (!email || !email.includes("@")) {
            alert("Email không hợp lệ!");
            return;
        }

        try {
            let response = await fetch("/Account/ForgotPassword", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `email=${encodeURIComponent(email)}`,
            });

            let result = await response.json();
            if (result.success) {
                step1.classList.add("d-none");
                step2.classList.remove("d-none");
                alert("Mã OTP đã được gửi!");
            } else {
                alert(result.message);
            }
        } catch (error) {
            console.error("Lỗi khi gửi OTP:", error);
            alert("Lỗi hệ thống, vui lòng thử lại sau!");
        }
    });

    // ✅ Xử lý xác thực OTP
    verifyOtpBtn.addEventListener("click", async function (e) {
        e.preventDefault();

        // Kiểm tra có đúng 6 ô nhập không
        const otpInputs = document.querySelectorAll(".verification-input");
        if (otpInputs.length !== 6) {
            alert("Có lỗi xảy ra với ô nhập OTP!");
            return;
        }

        // 🔹 Ghép các số OTP thành một chuỗi duy nhất
        const otp = Array.from(otpInputs)
            .map(input => input.value.trim()) // Xóa khoảng trắng
            .join(""); // Nối thành chuỗi OTP đầy đủ

        console.log("OTP gửi lên server:", otp); // Debug để kiểm tra giá trị OTP trước khi gửi

        // Kiểm tra OTP nhập đủ 6 số chưa
        if (otp.length !== 6 || isNaN(otp)) {
            alert("Vui lòng nhập đủ 6 số OTP hợp lệ!");
            return;
        }

        try {
            let response = await fetch("/Account/VerifyOTP", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `otp=${encodeURIComponent(otp)}`,
            });

            let result = await response.json();
            if (result.success) {
                step2.classList.add("d-none");
                step3.classList.remove("d-none");
                alert("Xác thực OTP thành công!");
            } else {
                alert(result.message);
            }
        } catch (error) {
            console.error("Lỗi khi xác thực OTP:", error);
            alert("Lỗi hệ thống, vui lòng thử lại sau!");
        }
    });

    //Xử lý đặt lại mật khẩu (Dùng sự kiện `submit`)
    resetPasswordForm.addEventListener("submit", async function (e) {
        e.preventDefault(); // Ngăn form gửi dữ liệu mặc định

        const newPassword = newPasswordInput.value.trim();
        const confirmPassword = confirmPasswordInput.value.trim();

        // Kiểm tra độ dài mật khẩu
        if (newPassword.length < 8) {
            alert("Mật khẩu phải có ít nhất 8 ký tự!");
            return;
        }

        // Kiểm tra mật khẩu xác nhận
        if (newPassword !== confirmPassword) {
            alert("Mật khẩu xác nhận không khớp!");
            return;
        }

        try {
            console.log("🔄 Đang gửi yêu cầu đặt lại mật khẩu...");

            //Gửi yêu cầu đặt lại mật khẩu bằng `application/x-www-form-urlencoded`
            let formData = new URLSearchParams();
            formData.append("newPassword", newPassword);
            formData.append("confirmPassword", confirmPassword);

            let response = await fetch("/Account/ResetPassword", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: formData,
            });

            let result = await response.json();
            console.log("✅ Phản hồi từ server:", result);

            if (result.success) {
                alert("Mật khẩu đã được đặt lại thành công!");
                window.location.href = result.redirectUrl; // Chuyển hướng đến trang đăng nhập
            } else {
                alert(result.message);
            }
        } catch (error) {
            console.error("❌ Lỗi khi đặt lại mật khẩu:", error);
            alert("Lỗi hệ thống, vui lòng thử lại sau!");
        }
    });
});
