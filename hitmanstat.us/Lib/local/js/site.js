/**
 * Mithril Namespaces
 */
var services = services || {};

/**
 * Views DOM anchors
 */
var servicesView = document.getElementById('services-container');
var timerView = document.getElementById('next-refresh');

/**
 * DOM Elements
 */
var errorElement = document.getElementById('backend-error-container');

/**
 * Notifications
 */
var showNotification = function (type, title, message) {

    toastr.options = {
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-center",
        "timeOut": "10000",
        "preventDuplicates": true
    }

    switch (type) {
        case "success":
            toastr.success(message, title);
            break;
        case "info":
            toastr.info(message, title);
            break;
        case "warning":
            toastr.warning(message, title);
            break;
        case "error":
            toastr.error(message, title);
            break;
        default:
    }

};

/**
 * Tooltips
 */
$(function () {
    $('[data-toggle="tooltip"]').tooltip();
});