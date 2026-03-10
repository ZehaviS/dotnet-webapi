const uri = '/saleds';
let saleds = [];

function getAuthHeaders() {
    const token = localStorage.getItem('token');
    const headers = {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
    };
    if (token) headers['Authorization'] = `Bearer ${token}`;
    return headers;
}

function getItems() {
    console.log('getItems: fetching items...');
    fetch(uri, { headers: getAuthHeaders() })
        .then(response => response.json())
        .then(data => {
            console.log('getItems: received data', data);
            _displayItems(data);
        })
        .catch(error => console.error('Unable to get items.', error));
}

function addItem() {
    const addNameTextbox = document.getElementById('add-name');
    const imageFileInput = document.getElementById('add-imagefile');

    const item = {
        Name: addNameTextbox.value.trim(),
        weight: parseFloat(document.getElementById('add-weight').value.trim()),
        ImageUrl: ""
    };

    // If a file is selected, send multipart form-data so server can save it.
    if (imageFileInput && imageFileInput.files && imageFileInput.files.length > 0) {
        const file = imageFileInput.files[0];
        const formData = new FormData();
        formData.append('Name', item.Name);
        formData.append('weight', item.weight);
        formData.append('ImageUrl', item.ImageUrl);
        formData.append('imageFile', file);

        console.log('addItem: sending FormData with file', file.name);
        fetch(uri, {
            method: 'POST',
            headers: { 'Authorization': getAuthHeaders()['Authorization'] },
            body: formData
        })
            .then(response => {
                console.log('addItem response (FormData)', response.status, response.statusText);
                if (!response.ok) {
                    return response.text().then(text => Promise.reject(new Error(text)));
                }
                return response.json();
            })
            .then(() => {
                getItems();
                addNameTextbox.value = '';
                imageFileInput.value = '';
            })
            .catch(error => console.error('Unable to add item.', error));
    } else {
        console.log('addItem: sending JSON', item);
        fetch(uri, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify(item)
        })
            .then(response => {
                console.log('addItem response (JSON)', response.status, response.statusText);
                if (!response.ok) {
                    return response.text().then(text => Promise.reject(new Error(text)));
                }
                return response.json();
            })
            .then(() => {
                getItems();
                addNameTextbox.value = '';
            })
            .catch(error => console.error('Unable to add item.', error));
    }
}

function deleteItem(id) {
    fetch(`${uri}/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
    })
        .then(() => getItems())
        .catch(error => console.error('Unable to delete item.', error));
}

function displayEditForm(id) {
    const item = saleds.find(item => item.id === id);
    if (!item) {
        console.error('Item not found for edit:', id, saleds);
        return;
    }
    const editName = document.getElementById('edit-name');
    const editId = document.getElementById('edit-id');
    const editWeight = document.getElementById('edit-weight');
    const editImageUrl = document.getElementById('edit-imageurl');
    const editFormDiv = document.getElementById('editForm');
    console.log('DEBUG: editName', editName, 'editId', editId, 'editWeight', editWeight, 'editImageUrl', editImageUrl, 'editFormDiv', editFormDiv);
    if (!editName || !editId || !editWeight || !editImageUrl || !editFormDiv) {
        console.error('Edit form fields missing in DOM:', {editName, editId, editWeight, editImageUrl, editFormDiv});
        alert('Edit form is not available on this page.');
        return;
    }
    editName.value = item.Name || item.name || '';
    editId.value = item.id;
    editWeight.value = item.weight;
    editImageUrl.value = item.ImageUrl || item.imageUrl || '';
    editFormDiv.style.display = 'block';
    console.log('Opening edit form for:', item);
}

function updateItem() {
    const itemId = document.getElementById('edit-id').value;
    const name = document.getElementById('edit-name').value.trim();
    const weight = parseFloat(document.getElementById('edit-weight').value.trim());
    const imageUrl = document.getElementById('edit-imageurl').value.trim() || "";
    const imageFileInput = document.getElementById('edit-imagefile');
    const item = {
        id: parseInt(itemId, 10),
        Name: name,
        weight: weight,
        ImageUrl: imageUrl
    };

    // Handle file upload
    if (imageFileInput && imageFileInput.files && imageFileInput.files.length > 0) {
        const file = imageFileInput.files[0];
        const formData = new FormData();
        formData.append('id', item.id);
        formData.append('Name', item.Name);
        formData.append('weight', item.weight);
        formData.append('ImageUrl', item.ImageUrl);
        formData.append('imageFile', file);

        console.log('updateItem: sending FormData update', { item, file });
        fetch(`${uri}/${itemId}`, {
            method: 'PUT',
            headers: { 'Authorization': getAuthHeaders()['Authorization'] },
            body: formData
        })
            .then(response => {
                console.log('updateItem response (FormData)', response.status, response.statusText);
                if (!response.ok) {
                    return response.text().then(text => Promise.reject(new Error(text)));
                }
                return response;
            })
            .then(() => getItems())
            .catch(error => console.error('Unable to update item.', error));
    } else {
        console.log('updateItem: sending JSON update', item);
        fetch(`${uri}/${itemId}`, {
            method: 'PUT',
            headers: getAuthHeaders(),
            body: JSON.stringify(item)
        })
            .then(response => {
                console.log('updateItem response (JSON)', response.status, response.statusText);
                if (!response.ok) {
                    return response.text().then(text => Promise.reject(new Error(text)));
                }
                return response;
            })
            .then(() => getItems())
            .catch(error => console.error('Unable to update item.', error));
    }

    closeInput();
    return false;
}

function closeInput() {
    document.getElementById('editForm').style.display = 'none';
}

function _displayCount(itemCount) {
    const name = (itemCount === 1) ? 'salad' : 'salad kinds';

    document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function _displayItems(data) {
    const tBody = document.getElementById('saleds');
    tBody.innerHTML = '';

    saleds = data;

    _displayCount(data.length);

    const button = document.createElement('button');

    data.forEach(item => {
        let editButton = button.cloneNode(false);
        editButton.innerText = 'Edit';
        editButton.className = 'btn-edit';
        editButton.setAttribute('onclick', `displayEditForm(${item.id})`);

        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Delete';
        deleteButton.className = 'btn-delete';
        deleteButton.setAttribute('onclick', `deleteItem(${item.id})`);

        let tr = tBody.insertRow();

        let tdImage = tr.insertCell(0);
        const img = document.createElement('img');
        // תמיכה ב-ImageUrl מהשרת
        img.src = item.ImageUrl || item.imageUrl || 'img/5.jpg';
        img.alt = item.Name || item.name;
        img.className = 'salad-image';
        tdImage.appendChild(img);

        let td1 = tr.insertCell(1);
        td1.appendChild(document.createTextNode(item.name));

        let td2 = tr.insertCell(2);
        td2.appendChild(document.createTextNode(item.weight));

        let td3 = tr.insertCell(3);
        td3.appendChild(editButton);
        td3.appendChild(deleteButton);
    });


}

// SignalR: connect to activity hub and show fun notifications
function showActivityNotification(message) {
    let notif = document.createElement('div');
    notif.className = 'activity-popup';
    notif.innerText = message;
    document.body.appendChild(notif);
    setTimeout(() => {
        notif.classList.add('show');
        setTimeout(() => {
            notif.classList.remove('show');
            setTimeout(() => notif.remove(), 500);
        }, 4000);
    }, 100);
}

function setupSignalR() {
    const token = localStorage.getItem('token');
    if (!window.signalR || !token) return;
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/activityHub', {
            accessTokenFactory: () => token
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // לוגים לחיבור SignalR
    console.log('SignalR: token', token);
    connection.onclose(function (error) {
        console.log('SignalR: connection closed', error);
    });
    connection.onreconnecting(function (error) {
        console.log('SignalR: reconnecting', error);
    });
    connection.onreconnected(function (connectionId) {
        console.log('SignalR: reconnected', connectionId);
    });

    connection.on('ReceiveActivity', function (message) {
        showActivityNotification(message);
        getItems(); // עדכון הגריד מיד
    });

    connection.on('DebugLog', function (msg) {
        console.log('SignalR DebugLog:', msg);
        let debugDiv = document.getElementById('signalr-debug');
        if (!debugDiv) {
            debugDiv = document.createElement('div');
            debugDiv.id = 'signalr-debug';
            debugDiv.style.position = 'fixed';
            debugDiv.style.bottom = '10px';
            debugDiv.style.left = '10px';
            debugDiv.style.background = '#eee';
            debugDiv.style.padding = '8px';
            debugDiv.style.zIndex = 9999;
            document.body.appendChild(debugDiv);
        }
        debugDiv.innerText = msg;
    });

    connection.start()
        .then(() => {
            console.log('SignalR connection started successfully');
        })
        .catch(function (err) {
            console.error('SignalR connection error:', err);
        });
}

// load items when the script is first evaluated
getItems();
setupSignalR();