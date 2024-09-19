function showAlert(status,title,text) {
    if (status === "success") {
        Swal.fire({
            title: title,
            text: text,
            icon: "success",
        });
    } else if (status === "warning") {
        Swal.fire({
            title: title,
            text: text,
            icon: "warning",
        });
    } else if (status === "error") {
        Swal.fire({
            title: title,
            text: text,
            icon: "error",
        });
    }
}
function showModal(modalId) {
    var modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.show();
}