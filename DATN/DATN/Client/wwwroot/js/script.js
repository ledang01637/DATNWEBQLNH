
function checkTokenExpiry() {
    const token = localStorage.getItem('authToken');
    const expiryTime = localStorage.getItem('expiryTime');

    if (!token || !expiryTime) {
        return; 
    }

    const currentTime = new Date().toISOString();
    if (new Date(currentTime) > new Date(expiryTime)) {
        localStorage.removeItem('authToken');
        localStorage.removeItem('userName');
        localStorage.removeItem('expiryTime');
    }
}

function checkCartExpiry() {
    const token = localStorage.getItem('historyOrder');
    const expiryTime = localStorage.getItem('cartExpiryTime');

    if (!token || !expiryTime) {
        return;
    }

    const currentTime = new Date().toISOString();
    if (new Date(currentTime) > new Date(expiryTime)) {
        localStorage.removeItem('historyOrder');
        localStorage.removeItem('cartExpiryTime');
    }
}

function checkTokenExpiry() {
    const token = localStorage.getItem('authToken');
    const expiryTime = localStorage.getItem('expiryTime');

    if (!token || !expiryTime) {
        return; 
    }

    const currentTime = new Date().toISOString();
    if (new Date(currentTime) > new Date(expiryTime)) {
        localStorage.removeItem('authToken');
        localStorage.removeItem('userName');
        localStorage.removeItem('expiryTime');
    }
}

function showModal(modalId) {
    var modalElement = document.getElementById(modalId);
    if (modalElement) {
        var modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
}
function closeModal(modalId) {
    console.log('ab');
    $('#'+modalId).modal('hide');
}

window.cartFunctions = {
    getCart: function () {
        return sessionStorage.getItem('cart');
    },
    saveCart: function (cart) {
        sessionStorage.setItem('cart', cart);
    }
};
window.generateMD5Hash = function (input) {
    return CryptoJS.MD5(input).toString();
};
