
let sortableInstances = {};
function MoveTable(FloorId, IsSwap, dotNetHelper, maxColumns) {
    var sortableElement = document.getElementById("floor-" + FloorId);
    if (sortableInstances[FloorId]) {
        sortableInstances[FloorId].destroy();
    }
    if (sortableElement) {
        sortableInstances[FloorId] = new Sortable(sortableElement, {
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

                let newColumn = (toIndex % maxColumns) + 1;
                let newRow = Math.floor(toIndex / maxColumns) + 1;

                const itemEl = evt.item;
                const tableId = parseInt(itemEl.getAttribute('data-id'));
                const newPosition = `Hàng ${newRow} - Cột ${newColumn}`;
                const oldFloorId = parseInt(sortableElement.getAttribute('data-floor-id'));
                const newFloorId = parseInt(evt.to.getAttribute('data-floor-id'));

                itemEl.setAttribute('data-position', newPosition);

                if (oldFloorId !== newFloorId) {

                    dotNetHelper.invokeMethodAsync('MoveFloor', tableId, newPosition, newFloorId)
                        .then(() => console.log('Position moved to new floor successfully'))
                        .catch(err => console.error('Error calling UpdateTablePosition:', err));
                } else {
                    // Lấy thông tin bàn swap
                    const swappedItemEl = evt.from.children[fromIndex];
                    const swappedTableId = parseInt(swappedItemEl.getAttribute('data-id'));
                    const swappedPosition = swappedItemEl.getAttribute('data-position');

                    const swappedNewColumn = (fromIndex % maxColumns) + 1;
                    const swappedNewRow = Math.floor(fromIndex / maxColumns) + 1;
                    const swappedNewPosition = `Hàng ${swappedNewRow} - Cột ${swappedNewColumn}`;

                    dotNetHelper.invokeMethodAsync('UpdateTablePosition', tableId, newPosition, newFloorId, swappedTableId, swappedNewPosition)
                        .then(() => console.log('Positions updated successfully'))
                        .catch(err => console.error('Error calling UpdateTablePosition:', err));
                }
            }


            //onEnd: function (evt) {
            //    const fromIndex = evt.oldIndex;
            //    const toIndex = evt.newIndex;

            //    const maxColumns = 6;
            //    let newColumn = (toIndex % maxColumns) + 1;
            //    let newRow = Math.floor(toIndex / maxColumns) + 1;

            //    const itemEl = evt.item;
            //    const tableId = parseInt(itemEl.getAttribute('data-id'));
            //    const newPosition = `Hàng ${newRow} - Cột ${newColumn}`;
            //    const newFloorId = parseInt(sortableElement.getAttribute('data-floor-id'));
            //    itemEl.setAttribute('data-position', newPosition);

            //    // Gọi phương thức từ Blazor
            //    dotNetHelper.invokeMethodAsync('UpdateTablePosition', tableId, newPosition, newFloorId)
            //        .then(() => console.log('Position updated successfully'))
            //        .catch(err => console.error('Error calling UpdateTablePosition:', err));
            //}
        });
    } else {
        console.error("Cannot find sortable element with ID: floor-" + FloorId);
    }
}