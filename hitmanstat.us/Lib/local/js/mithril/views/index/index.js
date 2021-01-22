$(function () {

    var storagePrefix = "hitmanstatus_";
    var storageItemsUnselected = storagePrefix + 'items-unselected';
    var storageItemsSelected = storagePrefix + 'items-selected';
    var storageTopBanner = storagePrefix + 'top-banner';

    /*
     * TOP BANNER
     */

    if (localStorage.getItem(storageTopBanner) != 'hidden') {
        $('#top-banner').removeClass('d-none');
    }

    $('#hide-top-banner').click(function () {
        $('#top-banner').remove();
        localStorage.setItem(storageTopBanner, 'hidden');
    });

    /*
     * COLLAPSING BUTTONS
     */

    var unselected = "section-unselected";
    var selected = "section-selected";

    if (localStorage.getItem(storageItemsUnselected)) {
        var currentUnselectedItems = localStorage.getItem(storageItemsUnselected).split(";");
        $.each(currentUnselectedItems, function (index, value) {
            var id = "#" + value;
            if ($(id).hasClass(selected)) {
                $(id).removeClass(selected);
            }
            if (!$(id).hasClass(unselected)) {
                $(id).addClass(unselected);
                $(id).trigger("click");
            }
        });
    }

    if (localStorage.getItem(storageItemsSelected)) {
        var currentSelectedItems = localStorage.getItem(storageItemsSelected).split(";");
        $.each(currentSelectedItems, function (index, value) {
            var id = "#" + value;
            if ($(id).hasClass(unselected)) {
                $(id).removeClass(unselected);
            }
            if (!$(id).hasClass(selected)) {
                $(id).addClass(selected);
                $(id).trigger("click");
            }
        });
    }

    $("#services-container").removeClass('d-none');
    $("#section-btn-group").removeClass('d-none').addClass('d-flex');

    $('button[id$=-selector]').click(debounce(function () {

        if ($(this).hasClass(unselected)) {
            $(this).removeClass(unselected).addClass(selected);
        } else if ($(this).hasClass(selected)) {
            $(this).removeClass(selected).addClass(unselected);
        }

        var unselectedItems = "";
        var selectedItems = "";

        $('button[id$=-selector]').each(function () {
            if ($(this).hasClass(unselected)) {
                unselectedItems += $(this).attr('id') + ";";
            } else if ($(this).hasClass(selected)) {
                selectedItems += $(this).attr('id') + ";";
            }
        });

        localStorage.setItem(storageItemsUnselected, unselectedItems.slice(0, -1));
        localStorage.setItem(storageItemsSelected, selectedItems.slice(0, -1));

    }, 250));

});