function MoveTable() {
    console.log("ac");
    var sortableFloor1 = document.getElementById('sortable-floor-1');
    if (sortableFloor1) {
        new Sortable(sortableFloor1, {
            animation: 150,
            ghostClass: 'sortable-ghost',
            handle: '.grid-item',
            onEnd: function (evt) {
                let movedTableId = evt.item.getAttribute('data-table-id');
                let newRow = evt.newIndex % 6 + 1; // Ví dụ, bạn có thể tính toán vị trí mới theo cách bạn cần
                let newColumn = Math.floor(evt.newIndex / 6) + 1;
                console.log(`Moved table with ID: ${movedTableId} to new position: Row ${newRow} Column ${newColumn}`);
                // Cập nhật vị trí mới vào server hoặc trạng thái ứng dụng nếu cần
            }
        });
    }

    var sortableFloor2 = document.getElementById('sortable-floor-2');
    if (sortableFloor2) {
        new Sortable(sortableFloor2, {
            animation: 150,
            ghostClass: 'sortable-ghost',
            handle: '.grid-item'
        });
    }
}
