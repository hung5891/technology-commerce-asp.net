$('#myTab a').on('click', function(e) {
    e.preventDefault()
    $(this).tab('show')
});

/**
 * Datatable call
 */
$(document).ready(function() {
    $('#my-orders-table').DataTable();
});

/**
 * My account nav click
 */
$(document).ready(function() {
    $('.tg-tabs-content-wrapp .my-account-dashboard .card').click(function() {

        var ariaClick = $(this).attr('area-toggle');
        $('.tg-account .account-banner .nav-area  a#' + ariaClick).click();
    });
});