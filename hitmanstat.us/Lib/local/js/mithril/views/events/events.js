$(function () {
    $('[data-utcdate]').each(function () {
        var date = moment($(this).attr('data-utcdate'));
        $(this).html(date.fromNow());
    });
});