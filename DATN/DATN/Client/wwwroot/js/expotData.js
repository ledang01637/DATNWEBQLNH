function saveAsFile(fileName, contentBase64) {
    const link = document.createElement("a");
    link.download = fileName;
    link.href = `data:application/octet-stream;base64,${contentBase64}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
