let callQueue = [], isProcessingCall = false;
function settingCallEvent(call1, localVideo, remoteVideo, callButton, answerCallButton, endCallButton, rejectCallButton, callboxId, dotNetObjectReference) {
    call1.on('addremotestream', function (stream) {
        console.log('addremotestream');
        var remoteVideoElement = remoteVideo[0];
        if (remoteVideoElement) {
            remoteVideoElement.srcObject = stream;
            remoteVideo.show();
        }
    });

    call1.on('addlocalstream', function (stream) {
        console.log('addlocalstream');
        var localVideoElement = localVideo[0];
        if (localVideoElement) {
            localVideoElement.srcObject = stream;
            localVideo.show();
        }
    });

    call1.on('signalingstate', function (state) {
        console.log('signalingstate ', state);
        if (state.code === 3) {

        } else if (state.code === 4 || state.code === 5 || state.code === 6) {

            var localVideoElement = localVideo[0];
            var remoteVideoElement = remoteVideo[0];

            if (localVideoElement) localVideoElement.srcObject = null;
            if (remoteVideoElement) remoteVideoElement.srcObject = null;

            localVideo.hide();
            remoteVideo.hide();

            var callboxElement = callboxId[0];
            callboxElement.style.display = "none";
            $('#incoming-call-notice').hide();

            if (state.code === 5) {
                busyCallFromJs(dotNetObjectReference);
            }
            endCallFromJs(dotNetObjectReference);

        }
    });

    call1.on('mediastate', function (state) {
        console.log('mediastate ', state);
    });

    call1.on('info', function (info) {
        console.log('on info:' + JSON.stringify(info));
    });
}

function setupCall(token, callerId, calleeId, isCall, dotNetObjectReference) {

    var callButton = $('#btn-call');
    var answerCallButton = $('#btn-answer');
    var endCallButton = $('#btn-end');
    var rejectCallButton = $('#btn-reject');
    var localVideo = $('#localVideo');
    var remoteVideo = $('#remoteVideo');
    var callboxId = $('#call-box');
    const callMessage = $('#call-message');



    var currentCall = null;

    var client = new StringeeClient();

    client.connect(token);

    client.on('otherdeviceauthen', (data) => {

    });

    client.on('connect', function () {
        console.log('+++ connected!');
    });

    client.on('authen', function (res) {
        console.log('+++ on authen: ', res);
    });

    client.on('disconnect', function (res) {
        console.log('+++ disconnected');
    });

    // MAKE CALL
    if (isCall) {

        currentCall = new StringeeCall(client, callerId, calleeId, false);

        settingCallEvent(currentCall, localVideo, remoteVideo, callButton, answerCallButton, endCallButton, rejectCallButton, callboxId, dotNetObjectReference);

        currentCall.makeCall(function (res) {
            console.log('+++ call callback: ', res);
            if (res.message === 'SUCCESS') {
                document.dispatchEvent(new Event('connect_ok'));
            }
        });
    }

    // RECEIVE CALL
    client.on('incomingcall', function (incomingcall) {

        console.log(isProcessingCall);
        if (isProcessingCall) {
            incomingcall.reject();
            callQueue.push(incomingcall);
            return;
        }
        isProcessingCall = true;

        $('#incoming-call-notice').show();
        currentCall = incomingcall;
        const callerId = incomingcall.fromNumber;

        $('#caller-name').text(`Cuộc gọi từ bàn: ${callerId}`);

        settingCallEvent(currentCall, localVideo, remoteVideo, callButton, answerCallButton, endCallButton, rejectCallButton, callboxId, dotNetObjectReference);

        if (callboxId != undefined && callboxId != null) {
            var callboxElement = callboxId[0];
            callboxElement.style.display = "block";
            answerCallButton.show();
            rejectCallButton.show();
            endCallButton.hide();
            callButton.hide();
        } else {
            console.log("callboxId is not found");
        }
    });


    // Event handler for buttons
    answerCallButton.on('click', function () {
        isProcessingCall = true;
        $(this).hide();
        endCallButton.show();
        rejectCallButton.hide();
        if (currentCall != null) {
            currentCall.answer(function (res) {
                console.log('+++ answering call: ', res);
                callMessage.textContent = 'Đang nghe...';
            });
        }
    });

    rejectCallButton.on('click', function () {
        isProcessingCall = false;
        $(this).hide();
        callButton.show();
        answerCallButton.hide();
        if (currentCall != null) {
            currentCall.reject(function (res) {
                console.log('+++ reject call: ', res);
                var callboxElement = callboxId[0];
                callboxElement.style.display = "none";
            });
        }

    });

    endCallButton.on('click', function () {
        isProcessingCall = false;
        $(this).hide();
        callButton.show();
        answerCallButton.hide();
        if (currentCall != null) {
            currentCall.hangup(function (res) {
                console.log('+++ hangup: ', res);
                var callboxElement = callboxId[0];
                callboxElement.style.display = "none";
                endCallFromJs(dotNetObjectReference);
            });
        }
        $('#incoming-call-notice').hide();
    });

    document.addEventListener('connect_ok', function () {
        callButton.hide();
        endCallButton.show();
    });
};

function setupVideo(answerButtonId, callButtonId, remoteVideo, localVideo) {
    var localVideo = $('#localVideo');
    var remoteVideo = $('#remoteVideo');
    var answerButton = document.getElementById(answerButtonId);
    var callButton = document.getElementById(callButtonId);

    answerButton.addEventListener('click', function () {
        remoteVideo[0].muted = false;
        localVideo[0].play();
        remoteVideo[0].play();
    });

    callButton.addEventListener('click', function () {
        remoteVideo[0].muted = false;
        localVideo[0].play();
        remoteVideo[0].play();
    });
}

function endCallFromJs(dotNetHelper) {
    isProcessingCall = false;
    dotNetHelper.invokeMethodAsync('EndCall');
    $('#btn-answer').hide();
}

function busyCallFromJs(dotNetHelper) {
    dotNetHelper.invokeMethodAsync('BusyCall');
}

function callButtonManager(isClose) {
    var callBox = $('#call-box').get(0);
    callBox.style.display = 'block';
    if (isClose) {
        callBox.style.display = 'none';
    }
    $('#caller-name').text(`Nhập bàn bạn muốn gọi`);
    $('#btn-call').show();
    $('#toNumber').prop('hidden', false);
    $('#btn-end').hide();
    $('#btn-reject').hide();
    $('#btn-answer').hide();

}


// Cấu hình giao diện với hiệu ứng particles
function layout() {
    particlesJS("particles-js", {
        particles: {
            number: { value: 50, density: { enable: true, value_area: 800 } },
            color: { value: "#ffffff" },
            shape: { type: "circle", stroke: { width: 0, color: "#000000" } },
            opacity: { value: 0.5 },
            size: { value: 3, random: true },
            move: { enable: true, speed: 2, direction: "bottom", out_mode: "out" }
        },
        interactivity: {
            detect_on: "canvas",
            events: { onhover: { enable: false }, onclick: { enable: false }, resize: true }
        },
        retina_detect: true
    });
}




//// Xử lý sự kiện cuộc gọi V2
//let currentCall = null, callQueue = [], isProcessingCall = false;

//function setupCall(token, callerId, calleeId, isCall, dotNetHelper) {
//    const callButton = $('#btn-call'), answerButton = $('#btn-answer');
//    const endButton = $('#btn-end'), rejectButton = $('#btn-reject');
//    const localVideo = $('#localVideo'), remoteVideo = $('#remoteVideo');
//    const callboxElement = $('#call-box'), callMessage = $('#call-message');

//    const client = new StringeeClient();
//    client.connect(token);

//    client.on('authen', (res) => console.log('Authen status:', res));

//    if (isCall) {
//        initiateCall(client, callerId, calleeId, dotNetHelper, localVideo, remoteVideo, callboxElement);
//    }

//    client.on('incomingcall', (incomingcall) => {
//        if (isProcessingCall) {
//            incomingcall.reject();
//            callQueue.push(incomingcall);
//            return;
//        }
//        isProcessingCall = true;
//        handleIncomingCall(incomingcall, callButton, answerButton, endButton, rejectButton, dotNetHelper, localVideo, remoteVideo, callboxElement);
//    });

//    answerButton.on('click', () => answerCurrentCall(currentCall, callMessage, callButton, answerButton, rejectButton, endButton));
//    rejectButton.on('click', () => rejectCurrentCall(currentCall, callMessage, callButton, answerButton, rejectButton, endButton, callboxElement));
//    endButton.on('click', () => endCurrentCall(currentCall, callMessage, callButton, answerButton, rejectButton, endButton, callboxElement));

//    document.addEventListener('connect_ok', () => {
//        callButton.hide();
//        endButton.show();
//    });
//}

//function settingCallEvent(call, localVideo, remoteVideo, callboxElement, dotNetHelper) {
//    call.on('addremotestream', (stream) => {
//        remoteVideo[0].srcObject = stream;
//        remoteVideo.show();
//    });

//    call.on('addlocalstream', (stream) => {
//        localVideo[0].srcObject = stream;
//        localVideo.show();
//    });

//    call.on('signalingstate', (state) => {
//        if ([4, 5, 6].includes(state.code)) {
//            localVideo[0].srcObject = null;
//            remoteVideo[0].srcObject = null;
//            localVideo.hide();
//            remoteVideo.hide();
//            callboxElement.hide();
//            $('#incoming-call-notice').hide();
//            dotNetHelper.invokeMethodAsync('EndCall');
//            isProcessingCall = false;
//        }
//    });
//}

//function initiateCall(client, callerId, calleeId, dotNetHelper, localVideo, remoteVideo, callboxElement) {
//    const currentCall = new StringeeCall(client, callerId, calleeId, false);
//    settingCallEvent(currentCall, localVideo, remoteVideo, callboxElement, dotNetHelper);
//    currentCall.makeCall((res) => {
//        if (res.message === 'SUCCESS') document.dispatchEvent(new Event('connect_ok'));
//    });
//}

//function handleIncomingCall(incomingcall, callButton, answerButton, endButton, rejectButton, dotNetHelper, localVideo, remoteVideo, callboxElement) {
//    isProcessingCall = true;
//    $('#incoming-call-notice').show();
//    currentCall = incomingcall;
//    $('#caller-name').text(`Cuộc gọi từ bàn: ${incomingcall.fromNumber}`);
//    settingCallEvent(incomingcall, localVideo, remoteVideo, callboxElement, dotNetHelper);

//    callboxElement.show();
//    answerButton.show();
//    rejectButton.show();

//    endButton.hide();
//    callButton.hide();
//}

//function answerCurrentCall(currentCall, callMessage, callButton, answerButton, rejectButton, endButton) {
//    endButton.show();

//    answerButton.hide();
//    rejectButton.hide();
//    callButton.hide();

//    currentCall.answer((res) => {
//        isProcessingCall = false;
//        callMessage.text('Đang nghe...');
//    });
//}

//function rejectCurrentCall(currentCall, callMessage, callButton, answerButton, rejectButton, endButton, callboxElement) {
//    callMessage.text('');
//    callboxElement.hide();

//    callButton.show();
//    answerButton.hide();
//    rejectButton.hide();
//    endButton.hide();

//    currentCall.reject(() => {
//        isProcessingCall = false;
//    });
//}

//function endCurrentCall(currentCall, callMessage, callButton, answerButton, rejectButton, endButton, callboxElement) {
//    callMessage.text('');
//    callboxElement.hide();

//    callButton.show();
//    answerButton.hide();
//    rejectButton.hide();
//    endButton.hide();

//    currentCall.hangup(() => {
//        isProcessingCall = false;
//        $('#incoming-call-notice').hide();
//    });
//}

//function setupVideo(answerButtonId, callButtonId, remoteVideo, localVideo) {
//    var localVideo = $('#localVideo');
//    var remoteVideo = $('#remoteVideo');
//    var answerButton = document.getElementById(answerButtonId);
//    var callButton = document.getElementById(callButtonId);

//    answerButton.addEventListener('click', function () {
//        remoteVideo[0].muted = false;
//        localVideo[0].play();
//        remoteVideo[0].play();
//    });

//    callButton.addEventListener('click', function () {
//        remoteVideo[0].muted = false;
//        localVideo[0].play();
//        remoteVideo[0].play();
//    });
//}

//function callButtonManager(isClose) {
//    var callBox = $('#call-box').get(0);
//    callBox.style.display = 'block';
//    if (isClose) {
//        callBox.style.display = 'none';
//    }
//    $('#caller-name').text(`Nhập bàn bạn muốn gọi`);
//    $('#btn-call').show();
//    $('#toNumber').prop('hidden', false);
//    $('#btn-end').hide();
//    $('#btn-reject').hide();
//    $('#btn-answer').hide();

//}
//function endCallFromJs(dotNetHelper) {
//    dotNetHelper.invokeMethodAsync('EndCall');
//    $('#btn-answer').hide();
//}





