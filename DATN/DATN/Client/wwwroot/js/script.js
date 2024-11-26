
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

//Payment
function selectPaymentMethod(isCash, cashBtnId, transferBtnId) {
    document.querySelectorAll('.payment-button').forEach(button => {
        button.classList.remove('active');
        button.classList.add('text-muted');
    });
    if (isCash === 't') {
        document.getElementById(transferBtnId).classList.add('active');
        document.getElementById(transferBtnId).classList.remove('text-muted');
    } else if (isCash === 'c') {
        document.getElementById(cashBtnId).classList.add('active');
        document.getElementById(cashBtnId).classList.remove('text-muted');
    }
}



//Scroll
function initScrollToTop() {
    let scrollToTopBtn = document.getElementById("scrollToTopBtn");
    let isVisible = false;

    function checkScroll() {
        const scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
        if (scrollTop > 100 && !isVisible) {
            scrollToTopBtn.style.display = "block";
            isVisible = true;
        } else if (scrollTop <= 100 && isVisible) {
            scrollToTopBtn.style.display = "none";
            isVisible = false;
        }
    }

    window.addEventListener('scroll', function () {
        requestAnimationFrame(checkScroll);
    });
}

function scrollToTop() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
}


//Call
function initCallButton(callButtonId, expandButtonId, closeBtnId) {
    const callButton = document.getElementById(callButtonId);
    const expandButtons = document.getElementById(expandButtonId);
    const closeButton = document.getElementById(closeBtnId);

    let shiftX, shiftY, isDragging = false;
    let isExpanded = false;

    // Chức năng mở rộng thu nhỏ
    callButton.addEventListener('click', toggleExpand);

    closeButton.addEventListener('click', function () {
        expandButtons.style.display = 'none';
        isExpanded = false;
    });

    // Sự kiện kéo nút gọi
    callButton.onmousedown = function (event) {
        event.preventDefault();
        isDragging = false;
        shiftX = event.clientX - callButton.getBoundingClientRect().left;
        shiftY = event.clientY - callButton.getBoundingClientRect().top;

        document.addEventListener('mousemove', onMouseMove);
        document.onmouseup = function () {
            document.removeEventListener('mousemove', onMouseMove);
            document.onmouseup = null;
        };
    };

    function onMouseMove(event) {
        isDragging = true;
        moveAt(event.pageX, event.pageY);
    }

    // Sự kiện kéo cho thiết bị cảm ứng
    callButton.addEventListener('touchstart', function (event) {
        event.preventDefault();
        const touch = event.touches[0];
        shiftX = touch.clientX - callButton.getBoundingClientRect().left;
        shiftY = touch.clientY - callButton.getBoundingClientRect().top;

        document.addEventListener('touchmove', onTouchMove);
        document.addEventListener('touchend', function (event) {
            document.removeEventListener('touchmove', onTouchMove);
            document.removeEventListener('touchend', null);

            const deltaX = Math.abs(touch.clientX - event.changedTouches[0].clientX);
            const deltaY = Math.abs(touch.clientY - event.changedTouches[0].clientY);

            if (deltaX < 5 && deltaY < 5) {
                toggleExpand();
            }
        });
    });

    function onTouchMove(event) {
        const touch = event.touches[0];
        moveAt(touch.pageX, touch.pageY);
    }

    function moveAt(pageX, pageY) {
        const buttonRect = callButton.getBoundingClientRect();
        const viewportWidth = window.innerWidth;
        const viewportHeight = window.innerHeight;

        let newX = Math.min(Math.max(pageX - shiftX, 0), viewportWidth - buttonRect.width);
        let newY = Math.min(Math.max(pageY - shiftY - window.scrollY, 0), viewportHeight - buttonRect.height);

        callButton.style.left = newX + 'px';
        callButton.style.top = newY + 'px';
        expandButtons.style.left = newX + 'px';
        expandButtons.style.top = (newY + buttonRect.height + 10) + 'px';
    }

    function toggleExpand() {
        isExpanded = !isExpanded;
        expandButtons.style.display = isExpanded ? 'flex' : 'none';
    }

    callButton.ondragstart = function () {
        return false;
    };
}



