document.addEventListener("DOMContentLoaded", function () {
    const step1 = document.getElementById("step1");
    const step2 = document.getElementById("step2");
    const step3 = document.getElementById("step3");

    const sendOtpBtn = document.getElementById("sendOtpBtn");
    const verifyOtpBtn = document.getElementById("verifyOtpBtn");
    const resetPasswordForm = document.getElementById("resetPasswordForm");

    const emailInput = document.getElementById("emailInput");
    const otpHiddenInput = document.getElementById("otpHiddenInput");
    const newPasswordInput = document.getElementById("newPassword");
    const confirmPasswordInput = document.getElementById("confirmPassword");

    const lengthCheck = document.getElementById("lengthCheck");
    const uppercaseCheck = document.getElementById("uppercaseCheck");
    const specialCharCheck = document.getElementById("specialCharCheck");
    const numberCheck = document.getElementById("numberCheck");
    const resetPasswordError = document.getElementById("resetPasswordError");

    // ✅ Cập nhật thanh bước
    function updateStepIndicator(currentStep) {
        const steps = document.querySelectorAll(".step");
        const connectors = document.querySelectorAll(".step-connector");

        steps.forEach((step, index) => {
            step.classList.remove("active", "completed");
            if (index < currentStep) step.classList.add("completed");
            else if (index === currentStep) step.classList.add("active");
        });

        connectors.forEach((connector, index) => {
            if (index < currentStep) connector.classList.add("active");
            else connector.classList.remove("active");
        });
    }

    // ✅ Hiển thị/ẩn lỗi
    function showResetError(message) {
        resetPasswordError.textContent = message;
        resetPasswordError.style.display = "block";
    }

    function clearResetError() {
        resetPasswordError.textContent = "";
        resetPasswordError.style.display = "none";
    }

    // ✅ Kiểm tra điều kiện mật khẩu
    function validatePasswordRequirements(password) {
        const updateClass = (condition, element) => {
            if (condition) {
                element.classList.add("text-success");
                element.classList.remove("text-muted");
            } else {
                element.classList.remove("text-success");
                element.classList.add("text-muted");
            }
        };

        updateClass(password.length >= 8, lengthCheck);
        updateClass(/[A-Z]/.test(password), uppercaseCheck);
        updateClass(/[!@#$%^&*(),.?":{}|<>]/.test(password), specialCharCheck);
        updateClass(/\d/.test(password), numberCheck);
    }

    newPasswordInput.addEventListener("input", function () {
        validatePasswordRequirements(this.value);
        clearResetError();
    });

    confirmPasswordInput.addEventListener("input", clearResetError);

    // ✅ Gửi OTP
    sendOtpBtn.addEventListener("click", async function (e) {
        e.preventDefault();
        const email = emailInput.value.trim();

        if (!email || !email.includes("@")) {
            showResetError("Email không hợp lệ!");
            return;
        }

        try {
            const response = await fetch("/Account/ForgotPassword", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `email=${encodeURIComponent(email)}`
            });

            const result = await response.json();
            if (result.success) {
                step1.classList.add("d-none");
                step2.classList.remove("d-none");
                updateStepIndicator(1);
                clearResetError();
            } else {
                showResetError(result.message || "Email không tồn tại trong hệ thống!");
            }
        } catch (error) {
            console.error("Lỗi khi gửi OTP:", error);
            showResetError("Lỗi hệ thống, vui lòng thử lại sau!");
        }
    });

    // ✅ Xác thực OTP
    verifyOtpBtn.addEventListener("click", async function (e) {
        e.preventDefault();
        const otpInputs = document.querySelectorAll(".verification-input");
        const otp = Array.from(otpInputs).map(input => input.value.trim()).join("");

        if (otp.length !== 6 || isNaN(otp)) {
            showResetError("Vui lòng nhập đủ 6 số OTP hợp lệ!");
            return;
        }

        try {
            const response = await fetch("/Account/VerifyOTP", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `otp=${encodeURIComponent(otp)}`
            });

            const result = await response.json();
            if (result.success) {
                step2.classList.add("d-none");
                step3.classList.remove("d-none");
                updateStepIndicator(2);
                clearResetError();
            } else {
                showResetError(result.message || "Mã OTP không đúng!");
            }
        } catch (error) {
            console.error("Lỗi xác thực OTP:", error);
            showResetError("Lỗi hệ thống, vui lòng thử lại sau!");
        }
    });

    // ✅ Đặt lại mật khẩu
    resetPasswordForm.addEventListener("submit", async function (e) {
        e.preventDefault();
        const newPassword = newPasswordInput.value.trim();
        const confirmPassword = confirmPasswordInput.value.trim();

        if (newPassword.length < 8) {
            showResetError("Mật khẩu phải có ít nhất 8 ký tự!");
            return;
        }

        if (newPassword !== confirmPassword) {
            showResetError("Mật khẩu xác nhận không khớp!");
            return;
        }

        try {
            const formData = new URLSearchParams();
            formData.append("newPassword", newPassword);
            formData.append("confirmPassword", confirmPassword);

            const response = await fetch("/Account/ResetPassword", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: formData
            });

            const result = await response.json();
            if (result.success) {
                window.location.href = result.redirectUrl;
            } else {
                showResetError(result.message || "Đặt lại mật khẩu thất bại!");
            }
        } catch (error) {
            console.error("Lỗi đặt lại mật khẩu:", error);
            showResetError("Lỗi hệ thống, vui lòng thử lại sau!");
        }
    });
});

// ✅ Toggle hiện/ẩn mật khẩu
function togglePasswordVisibility(inputId) {
    const input = document.getElementById(inputId);
    const icon = input.nextElementSibling.querySelector("i");

    if (input.type === "password") {
        input.type = "text";
        icon.classList.remove("fa-eye");
        icon.classList.add("fa-eye-slash");
    } else {
        input.type = "password";
        icon.classList.remove("fa-eye-slash");
        icon.classList.add("fa-eye");
    }
}
