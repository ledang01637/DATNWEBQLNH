function settingCallEvent(call1, localVideo, remoteVideo, callButton, answerCallButton, endCallButton, rejectCallButton) {
    call1.on('addremotestream', function (stream) {
        console.log('addremotestream');
        var remoteVideoElement = remoteVideo.get(0);
        if (remoteVideoElement) {
            remoteVideoElement.srcObject = stream;
            remoteVideo.show();
        }
    });

    call1.on('addlocalstream', function (stream) {
        console.log('addlocalstream');
        var localVideoElement = localVideo.get(0);
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
            callButton.show();
            endCallButton.hide();
            rejectCallButton.hide();
            answerCallButton.hide();
            var localVideoElement = localVideo.get(0);
            var remoteVideoElement = remoteVideo.get(0);
            if (localVideoElement) localVideoElement.srcObject = null;
            if (remoteVideoElement) remoteVideoElement.srcObject = null;
            localVideo.hide();
            remoteVideo.hide();
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

function setupCall(token, callerId, calleeId) {
    var localVideo = $('#localVideo');
    var remoteVideo = $('#remoteVideo');
    var callButton = $('#callButton');
    var answerCallButton = $('#answerCallButton');
    var rejectCallButton = $('#rejectCallButton');
    var endCallButton = $('#endCallButton');

    var currentCall = null;

    var client = new StringeeClient();
    client.connect(token);

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
    callButton.on('click', function () {
        currentCall = new StringeeCall(client, callerId, calleeId, false);

        settingCallEvent(currentCall, localVideo, remoteVideo, callButton, answerCallButton, endCallButton, rejectCallButton);

        currentCall.makeCall(function (res) {
            console.log('+++ call callback: ', res);
            if (res.message === 'SUCCESS') {
                document.dispatchEvent(new Event('connect_ok'));
            }
        });
    });

    // RECEIVE CALL
    client.on('incomingcall', function (incomingcall) {
        $('#incoming-call-notice').show();
        currentCall = incomingcall;
        settingCallEvent(currentCall, localVideo, remoteVideo, callButton, answerCallButton, endCallButton, rejectCallButton);
        callButton.hide();
        answerCallButton.show();
        rejectCallButton.show();
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
            });
        }
    });

    rejectCallButton.on('click', function () {
        if (currentCall != null) {
            currentCall.reject(function (res) {
                console.log('+++ reject call: ', res);
            });
        }

        callButton.show();
        $(this).hide();
        answerCallButton.hide();
    });

    endCallButton.on('click', function () {
        if (currentCall != null) {
            currentCall.hangup(function (res) {
                console.log('+++ hangup: ', res);
            });
        }

        callButton.show();
        endCallButton.hide();
        $('#incoming-call-notice').hide();
    });

    document.addEventListener('connect_ok', function () {
        callButton.hide();
        endCallButton.show();
    });
};

// videoControls.js

function setupCallEvents(localVideoId, remoteVideoId, answerButtonId, callButtonId) {
    var localVideo = document.getElementById(localVideoId);
    var remoteVideo = document.getElementById(remoteVideoId);
    var answerButton = document.getElementById(answerButtonId);
    var callButton = document.getElementById(callButtonId);

    answerButton.addEventListener('click', function () {
        localVideo.muted = false;
        localVideo.play();
        remoteVideo.play(); // Ensure remote video is also played
    });

    callButton.addEventListener('click', function () {
        localVideo.muted = false;
        localVideo.play();
        remoteVideo.play(); // Ensure remote video is also played
    });
}

