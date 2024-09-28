function initializeIsotope() {
    var $grid = $(".grid").isotope({
        itemSelector: ".all",
        percentPosition: true,
        masonry: {
            columnWidth: ".all"
        }
    });

    var currentFilter = '*';

    $('.filters_menu li').click(function () {
        $('.filters_menu li').removeClass('active');
        $(this).addClass('active');
        currentFilter = $(this).attr('data-filter');

        filterProducts();
    });

    $('#searchIndex').on('input', function () {
        filterProducts();
    });

    function filterProducts() {
        var searchText = $('#searchIndex').val().toLowerCase();

        $grid.isotope({
            filter: function () {
                var $this = $(this);

                var productName = $this.find('h5').text().toLowerCase();

                var isInCategory = $this.is(currentFilter) || currentFilter === '*';
                var isInSearch = productName.includes(searchText);

                return isInCategory && isInSearch;
            }
        });
    }

    // Nice select
    $('select').niceSelect();

    // Owl carousel for client section
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
            0: {
                items: 1
            },
            768: {
                items: 2
            },
            1000: {
                items: 2
            }
        }
    });
}
