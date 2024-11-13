function printInvoicePreview() {
    var printContents = document.getElementById("invoice-preview").innerHTML;
    var originalContents = document.body.innerHTML;

    document.body.innerHTML = printContents;
    window.print();
    document.body.innerHTML = originalContents;
}
