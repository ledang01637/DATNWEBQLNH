
let sortableInstances = {};
function MoveTable(FloorId, IsSwap, dotNetHelper) {
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

                const maxColumns = 6;
                let newColumn = (toIndex % maxColumns) + 1;
                let newRow = Math.floor(toIndex / maxColumns) + 1;

                const itemEl = evt.item;
                const tableId = parseInt(itemEl.getAttribute('data-id'));
                const newPosition = `Hàng ${newRow} - Cột ${newColumn}`;
                const newFloorId = parseInt(sortableElement.getAttribute('data-floor-id'));
                itemEl.setAttribute('data-position', newPosition);
                console.log(`Table ${tableId} moved from index ${fromIndex} to ${toIndex}, new position: ${newPosition}`);

                // Gọi phương thức từ Blazor
                dotNetHelper.invokeMethodAsync('UpdateTablePosition', tableId, newPosition, newFloorId)
                    .then(() => console.log('Position updated successfully'))
                    .catch(err => console.error('Error calling UpdateTablePosition:', err));
            }
        });
    } else {
        console.error("Cannot find sortable element with ID: floor-" + FloorId);
    }
}

