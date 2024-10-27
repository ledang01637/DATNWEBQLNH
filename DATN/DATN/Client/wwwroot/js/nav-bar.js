function Navbar(overlayId, mySidebarId, isClose) {
    if (isClose) {
        document.getElementById(mySidebarId).style.left = "-250px";
        document.getElementById(overlayId).style.display = "none";
        return;
    }
    document.getElementById(mySidebarId).style.left = "0";
    document.getElementById(overlayId).style.display = "block";
}