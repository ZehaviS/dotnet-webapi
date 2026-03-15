function getAuthHeaders() {
    const token = localStorage.getItem('token');
    const headers = {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
    };
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return headers;
}

function decodeJwt(token) {
    if (!token) return null;
    const parts = token.split('.');
    if (parts.length < 2) return null;
    try {
        return JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
    } catch (err) {
        return null;
    }
}

function isAdmin() {
    const token = localStorage.getItem('token');
    const payload = decodeJwt(token);
    return payload?.type === 'Admin';
}

function setStatus(msg, isError = false) {
    const status = document.getElementById('status');
    status.textContent = msg;
    status.style.color = isError ? '#a00' : '#174';
}

function loadUsers() {
    const userBody = document.getElementById('users-body');
    userBody.innerHTML = '';

    if (!isAdmin()) {
        setStatus('Admin login required.', true);
        return;
    }

    fetch('/api/user', { headers: getAuthHeaders() })
        .then(resp => {
            if (!resp.ok) throw new Error('Authorization error or server issue');
            return resp.json();
        })
        .then(users => {
            users.forEach(u => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${u.id}</td>
                    <td>${u.name}</td>
                    <td>${u.clearanceLevel}</td>
                    <td>
                        <button class="btn-edit" onclick="startEdit(${u.id}, '${u.name}', ${u.clearanceLevel})">Edit</button>
                        <button class="btn-delete" onclick="deleteUser(${u.id})">Delete</button>
                        <button class="btn-add" onclick="openUserSalads(${u.id}, '${encodeURIComponent(u.name)}')">Open Salads</button>
                    </td>
                `;
                userBody.appendChild(tr);
            });
            setStatus(`Loaded ${users.length} users successfully.`);
        })
        .catch(err => {
            console.error(err);
            setStatus('Error loading users. Please verify admin access.', true);
        });
}

function createUser() {
    const name = document.getElementById('new-user-name').value.trim();
    const clearance = parseInt(document.getElementById('new-user-clearance').value.trim() || '0', 10);
    if (!name) {
        setStatus('Enter user name.', true);
        return;
    }

    fetch('/api/user', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({ Name: name, ClearanceLevel: clearance })
    })
    .then(resp => {
        if (!resp.ok) throw new Error('Failed to create user');
        return resp.json();
    })
    .then(() => {
        document.getElementById('new-user-name').value = '';
        document.getElementById('new-user-clearance').value = '';
        loadUsers();
    })
    .catch(err => {
        console.error(err);
        setStatus('Error creating user. Make sure you are admin.', true);
    });
}

function startEdit(id, name, clearance) {
    document.getElementById('edit-user-id').value = id;
    document.getElementById('edit-user-name').value = name;
    document.getElementById('edit-user-clearance').value = clearance;
    document.getElementById('edit-user-section').style.display = 'block';
    setStatus('Edit user ready.', false);
}

function cancelEdit() {
    document.getElementById('edit-user-section').style.display = 'none';
    setStatus('Edit cancelled.', false);
}

function saveUserEdit() {
    const id = parseInt(document.getElementById('edit-user-id').value, 10);
    const name = document.getElementById('edit-user-name').value.trim();
    const clearance = parseInt(document.getElementById('edit-user-clearance').value.trim() || '0', 10);
    if (!name) {
        setStatus('Enter a name to update.', true);
        return;
    }

    fetch(`/api/user/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify({ Id: id, Name: name, ClearanceLevel: clearance })
    })
    .then(resp => {
        if (!resp.ok) throw new Error('Failed to update user');
        document.getElementById('edit-user-section').style.display = 'none';
        loadUsers();
    })
    .catch(err => {
        console.error(err);
        setStatus('Error updating user.', true);
    });
}

function deleteUser(id) {
    if (!confirm('Delete this user?')) return;
    fetch(`/api/user/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
    })
    .then(resp => {
        if (!resp.ok) throw new Error('Failed to delete user');
        loadUsers();
    })
    .catch(err => {
        console.error(err);
        setStatus('Error deleting user.', true);
    });
}

function openUserSalads(id, encodedName) {
    window.location.href = `user-salads.html?userId=${id}&username=${encodedName}`;
}

loadUsers();