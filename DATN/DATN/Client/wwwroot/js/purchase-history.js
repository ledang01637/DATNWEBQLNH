function initializeSearchOrder() {
    var $grid = $(".grid");


    var $grid = $(".grid").isotope({
        itemSelector: ".all",
        percentPosition: true,
        masonry: {
            columnWidth: ".all"
        }
    });
    $('#searchOrders').on('input', function () {
        var searchText = $(this).val().toLowerCase();

        $grid.isotope({
            filter: function () {
                var $card = $(this); 
                var orderId = $card.find('.card-header h5').text().toLowerCase();
                var productNames = $card.find('.list-group-item .fw-bold').map(function () {
                    return $(this).text().toLowerCase();
                }).get();

                var isInSearch = orderId.includes(searchText) || productNames.some(name => name.includes(searchText));

                return isInSearch;
            }
        });
    });


    $('select').niceSelect();

    $(".client_owl-carousel").owlCarousel({
        loop: true,
        margin: 0,
        dots: false,
        nav: true,
        autoplay: true,
        autoplayHoverPause: true,
        navText: [
            '<i class="fa fa-angle-left" aria-hidden="true"></i>',
            '<i class="fa fa-angle-right" aria-hidden="true"></i>'
        ],
        responsive: {
            0: { items: 1 },
            768: { items: 2 },
            1000: { items: 2 }
        }
    });
}
