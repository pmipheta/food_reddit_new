function togglePw() {
    const input = document.getElementById('PasswordHash');
    const btn = document.getElementById('toggleBtn');
    if (input.type === 'password') {
        input.type = 'text';
        btn.textContent = '🙈';
    } else {
        input.type = 'password';
        btn.textContent = '👁';
    }
}