function MoveTable() {
    var sortableFloor1 = document.getElementById('sortable-floor1');
    console.log(sortableFloor1);  // In ra để kiểm tra phần tử có tồn tại không

    if (sortableFloor1) {
        new Sortable(sortableFloor1, {
            animation: 150,
            ghostClass: 'sortable-ghost',
            handle: '.grid-item'
        });
    }

    var sortableFloor2 = document.getElementById('sortable-floor2');
    if (sortableFloor2) {
        new Sortable(sortableFloor2, {
            animation: 150,
            ghostClass: 'sortable-ghost',
            handle: '.grid-item'
        });
    }
}
