document.addEventListener("DOMContentLoaded", function () {
    const step1 = document.getElementById("step1");
    const step2 = document.getElementById("step2");
    const step3 = document.getElementById("step3");
    const successStep = document.getElementById("success");

    const emailForm = step1.querySelector("form");
    const verifyButton = step2.querySelector("button");
    const resetPasswordForm = step3.querySelector("form");

    function updateStepIndicator(currentStep) {
        document.querySelectorAll(".step").forEach((step, index) => {
            if (index + 1 === currentStep) {
                step.classList.add("active");
            } else {
                step.classList.remove("active");
            }
        });
    }

    emailForm.addEventListener("submit", function (e) {
        e.preventDefault();
        step1.classList.add("d-none");
        step2.classList.remove("d-none");
        updateStepIndicator(2);
    });

    verifyButton.addEventListener("click", function () {
        step2.classList.add("d-none");
        step3.classList.remove("d-none");
        updateStepIndicator(3);
    });

    resetPasswordForm.addEventListener("submit", function (e) {
        e.preventDefault();
        step3.classList.add("d-none");
        successStep.classList.remove("d-none");
    });

    document.querySelectorAll(".verification-input").forEach((input, index, inputs) => {
        input.addEventListener("input", function () {
            if (this.value.length === 1 && index < inputs.length - 1) {
                inputs[index + 1].focus();
            }
        });
    });

    document.querySelectorAll(".password-toggle").forEach(toggle => {
        toggle.addEventListener("click", function () {
            let input = this.previousElementSibling;
            if (input.type === "password") {
                input.type = "text";
                this.innerHTML = '<i class="fas fa-eye-slash"></i>';
            } else {
                input.type = "password";
                this.innerHTML = '<i class="fas fa-eye"></i>';
            }
        });
    });

    const passwordInput = document.querySelector("input[type='password']");
    const strengthMeter = document.querySelector(".strength-meter");
    const requirements = document.querySelectorAll(".requirement-list li");

    passwordInput.addEventListener("input", function () {
        let value = passwordInput.value;
        let strength = 0;

        let checks = [
            { regex: /.{8,}/, element: requirements[0] },
            { regex: /[A-Z]/, element: requirements[1] },
            { regex: /[!@#$%^&*(),.?":{}|<>]/, element: requirements[2] },
            { regex: /[0-9]/, element: requirements[3] }
        ];

        checks.forEach(check => {
            if (check.regex.test(value)) {
                check.element.classList.add("valid");
                check.element.firstElementChild.className = "fas fa-check-circle me-2";
                strength++;
            } else {
                check.element.classList.remove("valid");
                check.element.firstElementChild.className = "fas fa-circle me-2";
            }
        });

        let strengthPercent = (strength / checks.length) * 100;
        strengthMeter.style.width = strengthPercent + "%";
        strengthMeter.className = "strength-meter bg-" + (strengthPercent > 75 ? "success" : strengthPercent > 50 ? "warning" : "danger");
    });
});
