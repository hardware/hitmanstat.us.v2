﻿/**
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
    $("body").tooltip({
        selector: '[data-toggle="tooltip"]'
    });
});

/**
 * Debounce function limits the rate at which a function can fire.
 */
var debounce = function (func, wait, immediate) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        var later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
};

/**
 * Get string hash code
 */
String.prototype.hashCode = function () {
    var hash = 0,
        i, chr;
    if (this.length === 0) return hash;
    for (i = 0; i < this.length; i++) {
        chr = this.charCodeAt(i);
        hash = ((hash << 5) - hash) + chr;
        hash |= 0; // Convert to 32bit integer
    }
    return hash;
}