
function MoveTable(FloorId,IsSwap) {
    var sortableElement = document.getElementById("floor-" + FloorId);

    if (sortableElement) {
        new Sortable(sortableElement, {
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
                if (evt.swapItem) {
                    console.log(IsSwap);
                } else {
                    console.log(IsSwap);
                }

                updateTablePositions();
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
