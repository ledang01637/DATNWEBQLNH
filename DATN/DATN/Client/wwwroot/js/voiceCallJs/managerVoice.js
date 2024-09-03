var localVideo = document.getElementById('localVideo');
var remoteVideo = document.getElementById('remoteVideo');
var btn = document.getElementById('answerCallButton');

btn.addEventListener('click', function () {
    localVideo.muted = false;
    remoteVideo.muted = false;
    localVideo.play();
});