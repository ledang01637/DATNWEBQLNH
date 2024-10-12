function layout() {
    particlesJS("particles-js", {
        "particles": {
            "number": {
                "value": 50,
                "density": {
                    "enable": true,
                    "value_area": 800
                }
            },
            "color": {
                "value": "#ffffff"
            },
            "shape": {
                "type": "circle",
                "stroke": {
                    "width": 0,
                    "color": "#000000"
                }
            },
            "opacity": {
                "value": 0.5,
                "random": false
            },
            "size": {
                "value": 3,
                "random": true
            },
            "move": {
                "enable": true,
                "speed": 2,
                "direction": "bottom",
                "random": false,
                "straight": false,
                "out_mode": "out",
                "bounce": false
            }
        },
        "interactivity": {
            "detect_on": "canvas",
            "events": {
                "onhover": {
                    "enable": false,
                    "mode": "repulse"
                },
                "onclick": {
                    "enable": false,
                    "mode": "push"
                },
                "resize": true
            }
        },
        "retina_detect": true
    });
}
function settingCallEvent(call1, localVideo, remoteVideo, callButton, answerCallButton, endCallButton, rejectCallButton, callboxId) {
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
            console.log("state code :" + state.code);
        } else if (state.code === 4 || state.code === 5 || state.code === 6) {
            console.log("state code :" + state.code);

            var localVideoElement = localVideo[0];
            var remoteVideoElement = remoteVideo[0];

            if (localVideoElement) localVideoElement.srcObject = null;
            if (remoteVideoElement) remoteVideoElement.srcObject = null;

            localVideo.hide();
            remoteVideo.hide();

            var callboxElement = callboxId[0];
            console.log(callboxId[0]);
            callboxElement.style.display = "none";

            $('#incoming-call-notice').hide();
        }
    });

    call1.on('mediastate', function (state) {
        console.log('mediastate ', state);
    });

    call1.on('info', function (info) {
        console.log('on info:' + JSON.stringify(info));
    });
}

function setupCall(token, callerId, calleeId, isCall) {
    var callButton = $('#btn-call');
    var answerCallButton = $('#btn-answer');
    var endCallButton = $('#btn-end');
    var rejectCallButton = $('#btn-reject');

    var localVideo = $('#localVideo');
    var remoteVideo = $('#remoteVideo');

    var callboxId = $('#call-box');
    const callMessage = $('#call-message');
    const callActions = $('#call-actions');

    var currentCall = null;

    var client = new StringeeClient();

    client.connect(token);

    client.on('otherdeviceauthen', (data) => {
        console.log('Another device authenticated:', data);
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

        settingCallEvent(currentCall, localVideo, remoteVideo, callButton, answerCallButton, endCallButton, rejectCallButton, callboxId);

        currentCall.makeCall(function (res) {
            console.log('+++ call callback: ', res);
            if (res.message === 'SUCCESS') {
                document.dispatchEvent(new Event('connect_ok'));
            }
        });
    }

    // RECEIVE CALL
    client.on('incomingcall', function (incomingcall) {

        $('#incoming-call-notice').show();
        currentCall = incomingcall;
        const callerId = incomingcall.fromNumber;

        $('#caller-name').text(`Cuộc gọi từ bàn số: ${callerId}`);

        settingCallEvent(currentCall, localVideo, remoteVideo, callButton, answerCallButton, endCallButton, rejectCallButton, callboxId);

        if (callboxId != undefined && callboxId != null) {
            var callboxElement = callboxId[0];

            callboxElement.style.display = "block";
        } else {
            console.log("callboxId is not found");
        }
    });


    // Event handler for buttons
    answerCallButton.on('click', function () {
        $(this).hide();
        rejectCallButton.hide();
        endCallButton.show();
        callButton.hide();
        if (currentCall != null) {
            currentCall.answer(function (res) {
                console.log('+++ answering call: ', res);
                callMessage.textContent = 'Đang nghe...';
                callActions.innerHTML = `
                <button class="btn-action btn-end" id="btn-end">
                    <i class="bi bi-telephone-down-fill" style="font-size: 24px;"></i>
                    <span>Kết thúc</span>
                </button>
            `;
            });
        }
    });

    rejectCallButton.on('click', function () {
        if (currentCall != null) {
            currentCall.reject(function (res) {
                console.log('+++ reject call: ', res);
            });
        }

    });

    endCallButton.on('click', function () {
        if (currentCall != null) {
            currentCall.hangup(function (res) {
                console.log('+++ hangup: ', res);
                var callboxElement = callboxId[0];
                callboxElement.style.display = "none";
            });
        }

        //callButton.show();
        //endCallButton.hide();
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

// Khởi tạo Particles.js



