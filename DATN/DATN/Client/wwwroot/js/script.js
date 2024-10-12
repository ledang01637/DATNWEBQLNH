
function checkTokenExpiry() {
    const token = localStorage.getItem('authToken');
    const expiryTime = localStorage.getItem('expiryTime');

    if (!token || !expiryTime) {
        return;
    }

    const currentTime = new Date().toISOString();
    if (new Date(currentTime) > new Date(expiryTime)) {
        removeItem();
    }
}

function removeItem() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userName');
    localStorage.removeItem('expiryTime');
    localStorage.removeItem('AccountId');
    localStorage.removeItem('ss');
    localStorage.removeItem('n');
    localStorage.removeItem('historyOrder');
    localStorage.removeItem('cartExpiryTime');
}

function checkCartExpiry() {
    const cart = localStorage.getItem('historyOrder');
    const expiryTime = localStorage.getItem('cartExpiryTime');

    if (!cart || !expiryTime) {
        return;
    }

    const currentTime = new Date().toISOString();
    if (new Date(currentTime) > new Date(expiryTime)) {
        localStorage.removeItem('historyOrder');
        localStorage.removeItem('cartExpiryTime');
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

function showLog(text) {
    console.log(text);
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


//Call
function selectCall(callButtonId, expandButtonId, callStaffBtnId, closeBtnId) {

    const callButton = document.getElementById(callButtonId);
    const expandButtons = document.getElementById(expandButtonId);
    let isExpanded = false;

    callButton.addEventListener('click', function () {
        if (isExpanded) {
            expandButtons.style.display = 'none';
        } else {
            expandButtons.style.display = 'flex';
        }
        isExpanded = !isExpanded;
    });

    document.getElementById(closeBtnId).addEventListener('click', function () {
        expandButtons.style.display = 'none';
        isExpanded = false;
    });
}

function initDrag(callButtonId, expandButtonId, callStaffBtnId, closeBtnId) {
    const callButton = document.getElementById(callButtonId);
    const expandButtons = document.getElementById(expandButtonId);
    let shiftX, shiftY;
    let isDragging = false;
    let initialX, initialY;

    function moveAt(pageX, pageY) {

        let newX = pageX - shiftX;
        let newY = pageY - shiftY - window.scrollY;

        const buttonRect = callButton.getBoundingClientRect();
        const viewportWidth = window.innerWidth;
        const viewportHeight = window.innerHeight;


        if (newX < 0) {
            newX = 0;
        }

        if (newX + buttonRect.width > viewportWidth) {
            newX = viewportWidth - buttonRect.width;
        }

        if (newY < 0) {
            newY = 0;
        }

        if (newY + buttonRect.height > viewportHeight) {
            newY = viewportHeight - buttonRect.height;
        }

        callButton.style.left = newX + 'px';
        callButton.style.top = newY + 'px';
        expandButtons.style.left = newX + 'px';
        expandButtons.style.top = (newY + buttonRect.height + 10) + 'px';
    }

    callButton.onmousedown = function (event) {
        event.preventDefault();
        isDragging = false;
        shiftX = event.clientX - callButton.getBoundingClientRect().left;
        shiftY = event.clientY - callButton.getBoundingClientRect().top;
        initialX = event.clientX;
        initialY = event.clientY;

        document.addEventListener('mousemove', onMouseMove);
        document.onmouseup = function (event) {
            document.removeEventListener('mousemove', onMouseMove);
            document.onmouseup = null;

            if (!isDragging) {

                selectCall(callButtonId, expandButtonId, callStaffBtnId, closeBtnId);
            }
        };
    };

    function onMouseMove(event) {
        isDragging = true;
        moveAt(event.pageX, event.pageY);
    }

    // Drag for touch devices
    callButton.addEventListener('touchstart', function (event) {
        event.preventDefault();

        const touch = event.touches[0];
        shiftX = touch.clientX - callButton.getBoundingClientRect().left;
        shiftY = touch.clientY - callButton.getBoundingClientRect().top;
        initialX = touch.clientX;
        initialY = touch.clientY;

        document.addEventListener('touchmove', onTouchMove);
        document.addEventListener('touchend', function (event) {
            document.removeEventListener('touchmove', onTouchMove);
            document.removeEventListener('touchend', null);

            const deltaX = Math.abs(initialX - event.changedTouches[0].clientX);
            const deltaY = Math.abs(initialY - event.changedTouches[0].clientY);

            if (deltaX < 5 && deltaY < 5) {
                selectCall(callButtonId, expandButtonId, callStaffBtnId, closeBtnId);
            }
        });
    });

    function onTouchMove(event) {
        const touch = event.touches[0];
        moveAt(touch.pageX, touch.pageY);
    }

    callButton.ondragstart = function () {
        return false;
    };
}




