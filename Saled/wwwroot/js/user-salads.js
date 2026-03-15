function getQueryParam(name) {
    const params = new URLSearchParams(window.location.search);
    return params.get(name);
}

function getAuthHeaders() {
    const token = localStorage.getItem('token');
    const headers = {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
    };
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return headers;
}

function setStatus(msg, isError = false) {
    const status = document.getElementById('status');
    status.textContent = msg;
    status.style.color = isError ? '#a00' : '#174';
}

function loadUserSalads() {
    const userId = getQueryParam('userId');
    const username = decodeURIComponent(getQueryParam('username') || '');
    const title = document.getElementById('user-title');
    title.innerText = `Salads of ${username || 'User'}`;

    if (!userId) {
        setStatus('userId not found in query string.', true);
        return;
    }

    fetch(`/api/item?userId=${userId}`, { headers: getAuthHeaders() })
        .then(resp => {
            if (!resp.ok) throw new Error('Error loading salads');
            return resp.json();
        })
        .then(salads => {
            const container = document.getElementById('salad-list');
            if (!Array.isArray(salads) || salads.length === 0) {
                container.innerHTML = '<p>No salads found for this user.</p>';
                setStatus('');
                return;
            }
            const html = `<table><thead><tr><th>Id</th><th>Salad Name</th><th>Weight</th><th>User</th></tr></thead><tbody>${salads.map(s => `<tr><td>${s.id}</td><td>${s.name}</td><td>${s.weight}</td><td>${s.userId}</td></tr>`).join('')}</tbody></table>`;
            container.innerHTML = html;
            setStatus(`Displayed ${salads.length} salads.`);
        })
        .catch(err => {
            console.error(err);
            setStatus('Error loading salads.', true);
        });
}

loadUserSalads();