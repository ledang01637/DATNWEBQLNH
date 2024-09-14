function showAlert(status) {
    if (status === "InputNull") {
        Swal.fire({
            title: "Đăng nhập thất bại",
            text: "Vui lòng nhập tài khoản và mật khẩu",
            icon: "error",
        });
    } else if (status === "False") {
        Swal.fire({
            title: "Đăng nhập thất bại",
            text: "Tài khoản hoặc mật khẩu không chính xác",
            icon: "error",
        });
    } else if (status === "Block") {
        Swal.fire({
            title: "Đăng nhập thất bại",
            text: "Tài khoản đã bị khóa",
            icon: "warning",
        });
    } else if (status == "True") {
        Swal.fire({
            title: "Đăng nhập thành công",
            icon: "success",
        });
    } else if (status === "NoRole") {
        Swal.fire({
            title: "Đăng nhập thất bại",
            text: "Tài khoản không có quyền truy cập",
            icon: "error",
        });
    } else if (status === "Register") {
        Swal.fire({
            title: "Đăng ký thành công",
            text: "Chúc mừng bạn đã đăng ký thành công",
            icon: "success",
        });
    } else if (status === "NotLogin") {
        Swal.fire({
            title: "Vui lòng đăng nhập",
            icon: "error",
        });
    } else if (status === "success") {
        Swal.fire({
            title: "Đăng ký thành công",
            icon: "success",
        });
    } else if (status === "InputPhoneExits") {
        Swal.fire({
            title: "Đăng ký thất bại ",
            text: "Số điện thoại đã tồn tại",
            icon: "error",
        });
    } else if (status === "Error") {
        Swal.fire({
            title: "Đăng ký thất bại ",
            text: "Đã có lỗi xảy ra vui lòng đăng ký lại sau ít phút",
            icon: "error",
        });
    } else if (status === "InputEmailExits") {
        Swal.fire({
            title: "Đăng ký thất bại",
            text: "Email đã tồn tại",
            icon: "warning",
        });
    } else if (status === "UpdateSuccessLogin") {
        Swal.fire({
            title: "Cập nhật thông tin thành công",
            icon: "success",
        });
    } else if (status === "ProductSuccess") {
        Swal.fire({
            title: "Thành công",
            icon: "success",
        });
    } else if (status === "ProductFail") {
        Swal.fire({
            title: "Thất bại",
            text: "Sản phẩm đã tồn tại",
            icon: "error",
        });
    }
}
function showModal(modalId) {
    var modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.show();
}