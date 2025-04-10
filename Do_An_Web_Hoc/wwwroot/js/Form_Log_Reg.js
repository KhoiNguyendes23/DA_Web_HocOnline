const container = document.querySelector('.container');
const registerBtn = document.querySelector('.register-btn');
const loginBtn = document.querySelector('.login-btn');

registerBtn.addEventListener('click', () => {
    container.classList.add('active');
})

loginBtn.addEventListener('click', () => {
    container.classList.remove('active');
})
document.querySelectorAll('.toggle-password').forEach(icon => {
    icon.addEventListener('click', () => {
        const input = icon.parentElement.querySelector('input'); // lấy input trong cùng .input-box
        if (input.type === 'password') {
            input.type = 'text';
            icon.classList.remove('bxs-lock-alt');
            icon.classList.add('bxs-lock-open-alt');
        } else {
            input.type = 'password';
            icon.classList.remove('bxs-lock-open-alt');
            icon.classList.add('bxs-lock-alt');
        }
    });
});

