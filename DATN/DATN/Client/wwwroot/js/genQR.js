function generateQrCode(text) {
    const qrcode = document.getElementById("qrcode");
    console.log(text);
    new QRCode(qrcode, {
        text: text,
        width: 300,
        height: 300,
    });
}

function clearQrCode() {
    document.getElementById("qrcode").innerHTML = "";
}

