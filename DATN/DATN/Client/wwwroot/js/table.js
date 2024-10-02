
let sortableInstances = {};
function MoveTable(FloorId,IsSwap) {
    var sortableElement = document.getElementById("floor-" + FloorId);
    if (sortableInstances[FloorId]) {
        sortableInstances[FloorId].destroy();
    }
    if (sortableElement) {
        sortableInstances[FloorId] =  new Sortable(sortableElement, {
            swap: IsSwap,
            swapClass: 'highlight',
            group: {
                name: 'shared',
                pull: true,
                put: true
            },
            handle: ".grid-item",
            animation: 150, 
            onEnd: function (evt) {
                const fromIndex = evt.oldIndex;
                const toIndex = evt.newIndex;

                // Xác định số lượng cột tối đa trong mỗi hàng
                const maxColumns = 6;

                // Tính cột và hàng của vị trí mới
                let newColumn = (toIndex % maxColumns) + 1; // Số cột bắt đầu từ 1
                let newRow = Math.floor(toIndex / maxColumns) + 1; // Hàng bắt đầu từ 1

                // Lấy element đã di chuyển
                const itemEl = evt.item;
                const tableId = parseInt(itemEl.getAttribute('data-id'));

                // Tạo vị trí mới theo định dạng "row - column"
                const newPosition = `Hàng ${newRow} - Cột ${newColumn}`;

                itemEl.setAttribute('data-position', newPosition);
                console.log(`Table ${tableId} moved from index ${fromIndex} to ${toIndex}, new position: ${newPosition}`);
                DotNet.invokeMethodAsync('DATN.Client', 'UpdateTablePosition', tableId, newPosition);
            }
        });
    } else {
        console.error("Cannot find sortable element with ID: floor-" + FloorId);
    }
}


function updateTablePositions() {
    const tables = document.querySelectorAll('.grid-item');

    tables.forEach((table, index) => {
        // Lấy vị trí row và column từ grid
        const row = window.getComputedStyle(table).getPropertyValue('grid-row-start');
        const column = window.getComputedStyle(table).getPropertyValue('grid-column-start');

        // Tạo vị trí mới theo định dạng "row - column"
        const newPosition = `${row.trim()} - ${column.trim()}`;
        table.setAttribute('data-position', newPosition);

        // Gọi phương thức Blazor để cập nhật vị trí bàn
        const tableId = parseInt(table.getAttribute('data-id'));
        DotNet.invokeMethodAsync('DATN.Client', 'UpdateTablePosition', tableId, newPosition);
    });
}
