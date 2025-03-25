document.addEventListener('DOMContentLoaded', function () {
    // Language switcher logic
    const langTrigger = document.getElementById('langTrigger');
    const langDropdown = document.querySelector('.custom-lang-dropdown');
    const langOptions = document.querySelectorAll('.lang-option');
    const langSelect = document.getElementById('langSelect');
    const langForm = document.getElementById('langForm');

    if (langTrigger && langDropdown) {
        // Toggle dropdown
        langTrigger.addEventListener('click', function (e) {
            e.stopPropagation();
            langDropdown.classList.toggle('show');
        });

        // Select option
        langOptions.forEach(option => {
            option.addEventListener('click', function () {
                const value = this.dataset.value;
                const flag = this.querySelector('img').src;
                const text = this.querySelector('span').textContent;

                // Update selected display
                langTrigger.querySelector('img').src = flag;
                langTrigger.querySelector('span').textContent = text;

                // Update hidden select
                langSelect.value = value;

                // Submit form
                langForm.submit();

                // Close dropdown
                langDropdown.classList.remove('show');
            });
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', function (e) {
            if (!e.target.closest('.custom-lang-container')) {
                langDropdown.classList.remove('show');
            }
        });
    }
});