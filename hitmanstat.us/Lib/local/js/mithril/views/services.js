﻿var services = services || {};
var dateFormat = 'YYYY.MM.DD hh:mmA';

services.oncreate = function () {
    window.addEventListener("load", function () {
        services.refresh();
    });
};

services.view = function () {
    return [
        setServicesGroup(services, "h2"),
        setServicesGroup(services, "h1"),
        setServicesGroup(services, "ot")
    ]
};

function setServicesGroup(services, groupName) {
    return m("div", { class: "card-deck" }, services.list.map(function (service) {
        if (service.group == groupName) {
            return [
                m("div", { class: "card" }, [
                    m("div", { class: setHeaderClass(service) }),
                    m("div", { class: "card-body d-flex align-items-center justify-content-center" }, [
                        setBody(service)
                    ]),
                    m("ul", { class: "list-group list-group-flush" }, [
                        setMaintenance(service),
                        setElusive(service)
                    ]),
                    m("div", { class: "card-footer" }, [
                        m("small", {
                            class: "text-muted"
                        }, "Updated " + moment(service.lastCheck).fromNow()),
                        setLink(service),
                        setReport(service)
                    ])
                ]),
                setElusiveModal(service),
                setMaintenanceModal(service),
            ]
        }
    }));
}

function setHeaderClass(service) {
    var status = (service.status) ? service.status.toLowerCase() : "loading";
    return "card-header " + status;
}

function setBody(service) {
    var status = (service.status) ? service.status.toLowerCase() : "loading";
    var title = (service.title) ? " - " + service.title : "";
    var label = (status == "up") ? "online" : status;
    return m("p", { class: "card-text" }, [
        m("span", { class: "card-span-title" }, m.trust(setCardTitle(service))),
        m("span", { class: "card-span-status " + status }, label + title),
    ]);
}

function setCardTitle(service) {
    if (service.name.indexOf("n pc") !== -1) {
        return '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" focusable="false" width="1em" height="1em" style="-ms-transform: rotate(360deg); -webkit-transform: rotate(360deg); transform: rotate(360deg);" preserveAspectRatio="xMidYMid meet" viewBox="0 0 32 32"><path d="M15.974 0C7.573 0 .682 6.479.031 14.714l8.573 3.547a4.521 4.521 0 0 1 2.552-.786c.083 0 .167.005.25.005l3.813-5.521v-.078c0-3.328 2.703-6.031 6.031-6.031s6.036 2.708 6.036 6.036a6.039 6.039 0 0 1-6.036 6.031h-.135l-5.438 3.88c0 .073.005.141.005.214c0 2.5-2.021 4.526-4.521 4.526c-2.177 0-4.021-1.563-4.443-3.635L.583 20.36c1.901 6.719 8.063 11.641 15.391 11.641c8.833 0 15.995-7.161 15.995-16s-7.161-16-15.995-16zm-5.922 24.281l-1.964-.813a3.413 3.413 0 0 0 1.755 1.667a3.404 3.404 0 0 0 4.443-1.833a3.38 3.38 0 0 0 .005-2.599a3.36 3.36 0 0 0-1.839-1.844a3.38 3.38 0 0 0-2.5-.042l2.026.839c1.276.536 1.88 2 1.349 3.276s-2 1.88-3.276 1.349zm15.219-12.406a4.025 4.025 0 0 0-4.016-4.021a4.02 4.02 0 1 0 0 8.042a4.022 4.022 0 0 0 4.016-4.021zm-7.026-.005c0-1.672 1.349-3.021 3.016-3.021s3.026 1.349 3.026 3.021c0 1.667-1.359 3.021-3.026 3.021s-3.016-1.354-3.016-3.021z" fill="#c8c2b6"/></svg> <svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" focusable="false" width="1em" height="1em" style="-ms-transform: rotate(360deg); -webkit-transform: rotate(360deg); transform: rotate(360deg);" preserveAspectRatio="xMidYMid meet" viewBox="0 0 32 32"><path d="M4.719 0C2.886 0 2.214.677 2.214 2.505v22.083c0 .209.011.401.027.579c.047.401.047.792.421 1.229c.036.052.412.328.412.328c.203.099.343.172.572.265l11.115 4.656c.573.261.819.371 1.235.355h.005c.421.016.667-.093 1.24-.355l11.109-4.656c.235-.093.369-.167.577-.265c0 0 .376-.287.412-.328c.375-.437.375-.828.421-1.229c.016-.177.027-.369.027-.573V2.506c0-1.828-.677-2.505-2.505-2.505zm17.808 4.145h.905c1.511 0 2.251.735 2.251 2.267v2.505H23.85V6.51c0-.489-.224-.713-.699-.713h-.312c-.489 0-.713.224-.713.713v7.749c0 .489.224.713.713.713h.349c.468 0 .692-.224.692-.713v-2.771h1.833v2.86c0 1.525-.749 2.276-2.265 2.276h-.921c-1.521 0-2.267-.756-2.267-2.276V6.425c0-1.525.745-2.281 2.267-2.281zm-16.251.106h4.151v1.703H8.14v3.468h2.204v1.699H8.14v3.697h2.319v1.704H6.276zm5.088 0h2.928c1.515 0 2.265.755 2.265 2.28v3.261c0 1.525-.751 2.276-2.265 2.276h-1.057v4.453h-1.871zm6.037 0h1.864v12.271h-1.864zm-4.172 1.65v4.52H14c.469 0 .693-.228.693-.719V6.619c0-.489-.224-.719-.693-.719zM8.088 19.437h.276l.063.011h.1l.052.016h.052l.047.015l.052.011l.041.011l.093.021l.053.015l.036.011l.041.016l.052.016l.036.015l.053.021l.047.021l.041.025l.047.021l.036.025l.053.027l.041.025l.041.021l.041.031l.043.027l.036.031l.125.095l-.032.041l-.036.036l-.032.037l-.036.041l-.025.036l-.032.037l-.036.036l-.032.041l-.025.036l-.037.043l-.031.036l-.036.041l-.032.037l-.025.041l-.037.036l-.031.043l-.036.036l-.032.036l-.036-.025l-.041-.037l-.043-.025l-.077-.052l-.047-.027l-.043-.025l-.047-.027l-.036-.021l-.041-.02l-.084-.032l-.052-.009l-.041-.011l-.047-.011l-.053-.011l-.052-.005h-.052l-.061-.011h-.1l-.052.005h-.052l-.052.016l-.041.011l-.047.016l-.047.009l-.043.021l-.052.021l-.072.052l-.043.025l-.036.032l-.036.025l-.037.032l-.025.036l-.043.036l-.052.073l-.025.041l-.021.047l-.025.037l-.027.047l-.016.047l-.02.041l-.016.052l-.005.052l-.015.048l-.011.052v.052l-.005.052v.12l.005.052v.041l.005.052l.009.047l.016.041l.005.053l.016.041l.015.036l.021.052l.027.052l.02.037l.052.083l.032.041l.025.037l.043.031l.025.036l.036.032l.084.063l.036.02l.041.027l.048.021l.052.02l.036.021l.104.031l.047.005l.052.016l.052.005h.224l.063-.005h.047l.053-.021l.052-.005l.052-.015l.041-.011l.047-.021l.041-.02l.047-.021l.032-.021l.041-.025v-.464h-.735v-.744h1.661v1.667l-.036.025l-.036.031l-.037.027l-.041.031l-.041.021l-.036.032l-.084.052l-.052.025l-.083.052l-.053.021l-.041.02l-.047.021l-.104.041l-.041.021l-.095.031l-.047.011l-.047.016l-.052.016l-.041.009l-.156.032l-.048.005l-.104.011l-.057.005l-.052.004l-.057.005h-.26l-.052-.009h-.052l-.052-.011h-.047l-.052-.016l-.152-.031l-.041-.016l-.047-.005l-.052-.021l-.095-.031l-.093-.041l-.052-.021l-.036-.021l-.052-.02l-.037-.032l-.052-.02l-.031-.027l-.041-.025l-.084-.063l-.041-.027l-.032-.031l-.041-.032l-.068-.067l-.036-.032l-.031-.036l-.037-.037l-.025-.041l-.032-.031l-.025-.043l-.032-.041l-.025-.036l-.027-.041l-.025-.048l-.021-.041l-.021-.047l-.02-.041l-.041-.095l-.016-.036l-.021-.047l-.011-.047l-.009-.041l-.011-.052l-.016-.048l-.011-.052l-.005-.041l-.009-.052l-.011-.093l-.011-.104v-.276l.011-.053v-.052l.016-.052v-.052l.015-.047l.016-.052l.021-.093l.015-.052l.016-.047l.063-.141l.02-.041l.021-.047l.027-.048l.02-.041l.027-.036l.052-.084l.031-.041l.032-.036l.025-.041l.068-.068l.031-.037l.037-.036l.031-.036l.043-.032l.072-.063l.041-.031l.043-.027l.036-.031l.041-.027l.043-.02l.047-.027l.052-.025l.036-.027l.052-.02l.047-.021l.047-.025l.043-.011l.052-.016l.041-.021l.047-.009l.047-.016l.052-.011l.043-.016l.052-.011h.052l.047-.015h.052L8 19.444h.047zm15.985.011h.276l.063.011h.099l.052.015h.057l.052.016l.093.021l.052.011l.047.009l.053.016l.047.016l.041.011l.047.015l.052.016l.041.021l.052.02l.048.021l.047.027l.036.02l.047.027l.047.02l.043.027l.047.031l.036.027l.084.063l.041.025l-.032.041l-.025.043l-.031.036l-.032.041l-.025.047l-.027.043l-.031.036l-.032.041l-.025.043l-.032.041l-.025.036l-.032.041l-.025.048l-.032.041l-.031.036l-.032.041l-.025.043l-.041-.032l-.048-.025l-.036-.027l-.041-.025l-.047-.021l-.043-.027l-.047-.02l-.036-.021l-.052-.02l-.037-.021l-.041-.016l-.093-.031l-.104-.032l-.156-.031l-.052-.005l-.095-.011h-.109l-.057.011l-.052.011l-.047.011l-.041.02l-.037.021l-.041.036l-.031.047l-.021.048v.124l.027.057l.02.032l.032.031l.052.027l.041.025l.047.021l.052.02l.068.016l.036.016l.043.011l.052.011l.041.015l.047.011l.057.016l.052.016l.057.015l.057.011l.047.016l.057.015l.052.011l.047.011l.157.047l.041.016l.052.016l.047.02l.052.027l.104.041l.047.027l.084.052l.077.057l.048.031l.036.036l.036.043l.037.036l.025.036l.037.052l.025.037l.021.052l.02.031l.016.052l.016.043l.011.047l.02.104l.005.052l.005.047v.125l-.005.057l-.011.104l-.011.052l-.015.047l-.011.052l-.016.052l-.015.047l-.021.037l-.021.047l-.025.041l-.032.037l-.052.083l-.063.073l-.036.025l-.041.037l-.032.031l-.041.031l-.041.021l-.041.032l-.048.025l-.093.047l-.052.021l-.047.02l-.052.016l-.047.016l-.043.011l-.104.02l-.036.011l-.052.011h-.052l-.047.011h-.052l-.052.011h-.371l-.156-.016l-.052-.011l-.047-.005l-.104-.02l-.057-.011l-.047-.011l-.052-.016l-.053-.011l-.047-.015l-.052-.016l-.052-.021l-.041-.015l-.052-.016l-.052-.021l-.037-.02l-.052-.016l-.041-.027l-.052-.02l-.041-.027l-.037-.025l-.052-.027l-.036-.02l-.041-.032l-.041-.025l-.043-.032l-.036-.031l-.041-.032l-.037-.025l-.041-.037l.032-.041l.036-.036l.031-.037l.037-.041l.025-.036l.032-.041l.036-.037l.031-.036l.037-.041l.025-.037l.037-.036l.031-.041l.032-.037l.036-.041l.025-.036l.037-.037l.036-.041l.036.032l.048.031l.036.031l.052.027l.036.027l.047.031l.043.027l.047.02l.036.027l.047.015l.052.021l.043.021l.047.015l.041.021l.052.016l.047.015l.052.016l.052.005l.048.016l.052.005h.057l.047.015h.281l.047-.009l.052-.011l.036-.005l.043-.016l.036-.02l.047-.032l.027-.036l.02-.041l.016-.048v-.12l-.021-.047l-.025-.041l-.032-.031l-.047-.032l-.036-.015l-.047-.021l-.052-.021l-.057-.025l-.037-.011l-.041-.011l-.052-.016l-.036-.009l-.052-.016l-.052-.005l-.053-.021l-.052-.005l-.057-.015l-.047-.011l-.052-.016l-.052-.011l-.052-.015l-.047-.016l-.052-.011l-.041-.016l-.095-.031l-.052-.021l-.052-.015l-.104-.043l-.047-.025l-.052-.027l-.036-.025l-.048-.027l-.036-.025l-.047-.027l-.068-.068l-.036-.031l-.063-.073l-.027-.036l-.02-.036l-.032-.048l-.015-.036l-.048-.125l-.009-.052l-.011-.047v-.047l-.011-.052v-.213l.011-.104l.011-.043l.009-.047l.016-.041l.011-.052l.021-.036l.02-.053l.021-.041l.02-.052l.027-.036l.036-.041l.027-.043l.041-.036l.031-.036l.032-.043l.047-.036l.032-.027l.041-.031l.083-.052l.047-.027l.095-.047l.041-.015l.047-.016l.052-.021l.052-.015l.037-.011l.047-.011l.041-.011l.047-.011l.052-.011l.104-.009l.048-.005zm-12.318.036h.943l.043.095l.02.041l.016.052l.021.047l.015.041l.027.047l.031.095l.027.047l.041.093l.011.041l.083.188l.016.047l.021.043l.025.047l.011.047l.027.052l.009.047l.048.093l.02.037l.021.052l.016.052l.015.036l.027.052l.016.043l.02.052l.016.036l.021.052l.047.093l.015.047l.011.048l.021.047l.025.041l.021.052l.021.047l.015.041l.043.095l.015.047l.021.047l.016.047l.02.041l.027.048l.02.047l.021.041l.011.052l.041.093l.021.043l.015.047l.043.093l.025.052l.011.041l.027.053l.009.036l.021.052l.027.052l.02.036l.016.052l.021.043l.015.052l.027.036l.031.104l.021.037l.02.052l.027.041l.021.052l.009.047l.016.041l.021.047l.025.043h-1.041l-.025-.043l-.016-.047l-.021-.047l-.02-.052l-.011-.041l-.043-.093l-.015-.043l-.041-.093l-.016-.041l-.021-.052l-.031-.095l-.021-.041h-1.448l-.02.047l-.016.043l-.021.052l-.02.047l-.011.041l-.021.052l-.02.041l-.016.047l-.021.043l-.02.052l-.016.036l-.021.052l-.015.052l-.021.037l-.016.052h-1.031l.015-.048l.043-.093l.015-.052l.016-.041l.027-.047l.02-.047l.021-.043l.011-.047l.02-.052l.027-.041l.02-.047l.032-.095l.047-.093l.016-.047l.02-.041l.016-.048l.063-.14l.021-.052l.015-.041l.016-.047l.027-.043l.02-.052l.016-.047l.016-.041l.02-.052l.027-.037l.016-.052l.02-.041l.016-.047l.021-.052l.025-.041l.016-.052l.02-.037l.016-.052l.021-.052l.02-.036l.021-.052l.016-.043l.02-.052l.016-.036l.027-.052l.02-.052l.021-.041l.011-.047l.02-.048l.027-.047l.02-.041l.011-.052l.021-.047l.021-.043l.041-.093l.015-.041l.043-.104l.02-.037l.021-.052l.016-.041l.015-.052l.021-.047l.027-.041l.02-.052l.016-.037l.016-.052l.02-.041l.027-.047l.016-.052l.015-.043l.021-.052l.02-.036l.027-.052l.016-.052l.015-.036l.021-.052zm2.928.027h1.031l.032.041l.052.084l.025.047l.027.036l.025.047l.027.041l.025.048l.027.041l.025.036l.027.047l.025.043l.037.041l.015.041l.032.047l.025.043l.032.036l.021.047l.025.041l.032.043l.015.041l.037.047l.077.125l.021.041l.031.041l.027.041l.025.048l.079.124l.025.048l.027.041l.031-.041l.021-.053l.031-.036l.027-.047l.025-.036l.021-.052l.036-.037l.027-.047l.021-.036l.025-.043l.032-.047l.025-.036l.027-.052l.025-.036l.032-.048l.02-.036l.027-.052l.025-.031l.027-.043l.031-.052l.027-.036l.02-.047l.032-.037l.025-.052l.027-.031l.031-.041l.027-.052l.025-.037l.027-.047l.025-.036l.027-.052l.031-.037l.021-.047l.027-.036h1.047v3.719h-.98V21.04l-.025.037l-.032.052l-.025.031l-.032.041l-.02.052l-.032.037l-.025.036l-.032.052l-.052.073l-.031.041l-.027.052l-.031.037l-.027.036l-.02.052l-.032.036l-.025.037l-.032.052l-.025.036l-.032.041l-.025.047l-.021.037l-.031.041l-.027.047l-.031.036l-.032.043l-.02.041l-.027.047l-.031.037l-.032.041l-.02.052l-.037.031l-.02.041l-.032.053l-.025.036H16.6l-.031-.047l-.027-.043l-.025-.047l-.027-.036l-.031-.047l-.027-.041l-.031-.043l-.027-.041l-.025-.047l-.027-.036l-.036-.048l-.021-.041l-.031-.047l-.027-.036l-.025-.047l-.032-.043l-.025-.052l-.032-.036l-.025-.047l-.027-.043l-.025-.047l-.032-.036l-.025-.047l-.032-.041l-.02-.043l-.032-.041l-.025-.047l-.032-.036l-.025-.048l-.032-.041l-.02-.047l-.037-.036l-.02-.048l-.032-.041v2.193h-.963v-3.683zm4.624 0h2.933v.839h-1.959v.599h1.76v.792h-1.76v.635h1.984v.844h-2.953v-3.677zm-7.094 1.14l-.016.047l-.015.043l-.021.052l-.021.047l-.015.047l-.043.093l-.02.052l-.016.043l-.016.052l-.02.036l-.016.052l-.021.052l-.02.037l-.016.052l-.02.041l-.016.052l-.027.047l-.011.041l-.02.052l-.021.048l-.016.041l-.02.052h.859l-.02-.052l-.016-.047l-.041-.095l-.016-.047l-.021-.041l-.015-.052l-.021-.047l-.016-.047l-.02-.043l-.016-.047l-.021-.052l-.015-.041l-.043-.093l-.009-.048l-.021-.047l-.021-.052l-.015-.036l-.043-.104l-.015-.047zm-1.53 6.964h10.681l-5.452 1.797z" fill="#c8c2b6"/></svg><br />' + service.name;
    } else if (service.name.indexOf("2 pc") !== -1) {
        return '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" focusable="false" width="1em" height="1em" style="-ms-transform: rotate(360deg); -webkit-transform: rotate(360deg); transform: rotate(360deg);" preserveAspectRatio="xMidYMid meet" viewBox="0 0 32 32"><path d="M15.974 0C7.573 0 .682 6.479.031 14.714l8.573 3.547a4.521 4.521 0 0 1 2.552-.786c.083 0 .167.005.25.005l3.813-5.521v-.078c0-3.328 2.703-6.031 6.031-6.031s6.036 2.708 6.036 6.036a6.039 6.039 0 0 1-6.036 6.031h-.135l-5.438 3.88c0 .073.005.141.005.214c0 2.5-2.021 4.526-4.521 4.526c-2.177 0-4.021-1.563-4.443-3.635L.583 20.36c1.901 6.719 8.063 11.641 15.391 11.641c8.833 0 15.995-7.161 15.995-16s-7.161-16-15.995-16zm-5.922 24.281l-1.964-.813a3.413 3.413 0 0 0 1.755 1.667a3.404 3.404 0 0 0 4.443-1.833a3.38 3.38 0 0 0 .005-2.599a3.36 3.36 0 0 0-1.839-1.844a3.38 3.38 0 0 0-2.5-.042l2.026.839c1.276.536 1.88 2 1.349 3.276s-2 1.88-3.276 1.349zm15.219-12.406a4.025 4.025 0 0 0-4.016-4.021a4.02 4.02 0 1 0 0 8.042a4.022 4.022 0 0 0 4.016-4.021zm-7.026-.005c0-1.672 1.349-3.021 3.016-3.021s3.026 1.349 3.026 3.021c0 1.667-1.359 3.021-3.026 3.021s-3.016-1.354-3.016-3.021z" fill="#c8c2b6"/></svg><br />' + service.name;
    } else if (service.name.indexOf("xbox") !== -1) {
        return '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" focusable="false" width="1em" height="1em" style="-ms-transform: rotate(360deg); -webkit-transform: rotate(360deg); transform: rotate(360deg);" preserveAspectRatio="xMidYMid meet" viewBox="0 0 32 32"><path d="M5.469 28.041A15.907 15.907 0 0 0 16 32c4.036 0 7.719-1.489 10.536-3.959c2.5-2.547-5.755-11.609-10.536-15.219c-4.776 3.609-13.036 12.672-10.531 15.219zm14.88-19.202c3.333 3.948 9.979 13.749 8.104 17.213A15.924 15.924 0 0 0 32 16.005c0-4.453-1.817-8.484-4.76-11.38c0 0-.037-.032-.109-.057a1.056 1.056 0 0 0-.376-.057c-.785 0-2.645.577-6.405 4.328zM4.869 4.568c-.072.025-.109.057-.115.057a15.933 15.933 0 0 0-4.755 11.38c0 3.807 1.328 7.297 3.547 10.041c-1.864-3.468 4.771-13.265 8.109-17.208C7.895 5.082 6.03 4.51 5.244 4.51a.901.901 0 0 0-.376.063zM16 4.735s-3.927-2.297-6.995-2.407c-1.203-.041-1.937.391-2.027.453C9.838.86 12.879 0 15.978 0h.021c3.115 0 6.14.86 9.021 2.781c-.089-.063-.819-.495-2.027-.453c-3.068.109-6.995 2.401-6.995 2.401z" fill="#c8c2b6"/></svg><br />' + service.name;
    } else if (service.name.indexOf("ps4") !== -1) {
        return '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" focusable="false" width="1em" height="1em" style="-ms-transform: rotate(360deg); -webkit-transform: rotate(360deg); transform: rotate(360deg);" preserveAspectRatio="xMidYMid meet" viewBox="0 0 32 32"><path d="M11.979 3.464V26.86l5.219 1.681V8.917c0-.923.407-1.537 1.063-1.324c.844.245 1.011 1.089 1.011 2.011v7.833c3.256 1.589 5.817-.005 5.817-4.203c0-4.317-1.5-6.235-5.916-7.771c-1.745-.6-4.975-1.584-7.188-2zm6.209 21.656l8.396-3.037c.952-.343 1.099-.832.328-1.088c-.781-.255-2.183-.188-3.147.161l-5.604 2v-3.183l.317-.109s1.604-.561 3.885-.823c2.261-.239 5.048.041 7.251.88c2.464.803 2.724 1.964 2.099 2.767c-.62.796-2.161 1.38-2.161 1.38l-11.391 4.14v-3.063zm-15.776-.317c-2.537-.729-2.953-2.224-1.803-3.1c1.068-.776 2.875-1.4 2.875-1.4l7.489-2.683v3.083l-5.364 1.964c-.943.36-1.099.844-.317 1.099c.781.261 2.181.204 3.12-.156l2.583-.943v2.765c-.156.037-.339.057-.521.099c-2.583.439-5.323.261-8.047-.64z" fill="#c8c2b6"/></svg><br />' + service.name;
    } else if (service.name.indexOf("stadia") !== -1) {
        return '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" focusable="false" width="1em" height="1em" style="-ms-transform: rotate(360deg); -webkit-transform: rotate(360deg); transform: rotate(360deg);" preserveAspectRatio="xMidYMid meet" viewBox="0 0 32 32"><path d="M.271 11.557a.631.631 0 0 0-.271.521v.005c0 .089.021.172.052.255l3.068 6.938a.646.646 0 0 0 .802.349c2.005-.719 8.948-2.979 13.443-1.76c0 0-4.51.26-8.583 3.458a.647.647 0 0 0-.193.76l1.375 3.104l.526 1.234a.398.398 0 0 0 .703.063c.953-1.432 2.557-2.146 4.099-2.771a23.227 23.227 0 0 1 4.906-1.401a21.538 21.538 0 0 1 5.427-.224a.634.634 0 0 0 .667-.443l1.385-4.427a.628.628 0 0 0-.234-.703c-1.542-1.12-7.656-4.875-18.401-3.365c0 0 9.167-5.271 20.813.521a.643.643 0 0 0 .896-.385l1.219-3.901a.531.531 0 0 0 .031-.182v-.021a.649.649 0 0 0-.323-.547c-1.464-.823-6.411-3.302-13.214-3.302c-5.219 0-11.526 1.453-18.193 6.224z" fill="#c8c2b6"/></svg><br />' + service.name;
    } else if (service.name.indexOf("auth") !== -1) {
        return '<i class="fas fa-user-lock"></i><br />' + service.name;
    } else if (service.name.indexOf("forum") !== -1) {
        return '<i class="fab fa-discourse"></i><br />' + service.name;
    }
}

function setLink(service) {
    if (service.url) {
        return m("a", {
            class: "btn btn-outline-secondary float-right", href: service.url
        }, "Website");
    }
}

function setReport(service) {
    if (service.ref != "hmfc") {
        var name = service.name.toUpperCase();
        if (service.status == "maintenance") {
            return m("button", {
                class: "btn btn-outline-warning btn-sm float-right",
                style: "cursor:not-allowed",
                title: "",
                id: "spinner-" + service.ref,
                "data-original-title": "This service is currently under maintenance, reporting is disabled.",
                "data-toggle": "tooltip",
                "data-placement": "bottom",
            }, "!")
        } else {
            return m("button", {
                class: "btn btn-outline-warning btn-sm float-right",
                title: "I want to report an issue on " + name,
                id: "spinner-" + service.ref,
                onclick: reportService,
                "data-service-name": name,
                "data-service-ref": service.ref,
                "data-service-state": service.status,
                "data-toggle": "tooltip",
                "data-placement": "bottom"
            }, "!")
        }
        
    }
}

function setElusive(service) {
    if (service.elusive && service.ref) {
        var modalId = "#" + service.ref + "_elusive";
        return m("li", {
            class: "list-group-item elusive-target bg-info",
            "data-toggle": "modal",
            "data-target": modalId
        }, "Elusive target : " + service.elusive.name);
    }
}

function setMaintenance(service) {
    if (service.nextWindow && service.ref) {
        var modalId = "#" + service.ref + "_maintenance";
        var start = moment(service.nextWindow.start);
        var state = (moment().isAfter(start)) ? "Maintenance in progress" : "Scheduled maintenance";
        return m("li", {
            class: "list-group-item maintenance",
            "data-toggle": "modal",
            "data-target": modalId
        }, state);
    }
}

function setElusiveModal(service) {
    if (service.elusive && service.ref) {
        var modalId = service.ref + "_elusive";
        var start = moment(service.elusive.nextWindow.start);
        var end = moment(service.elusive.nextWindow.end);
        var duration = moment.duration(start.diff(moment()));
        return m("div", { class: "modal fade", id: modalId, role: "dialog" },
            m("div", { class: "modal-dialog modal-dialog-centered modal-lg", role: "document" },
                m("div", { class: "modal-content" }, [
                    m("div", { class: "modal-header" }, [
                        m("h5", { class: "modal-title" }, service.elusive.name),
                        m("button", { class: "close", type: "button", "data-dismiss": "modal" },
                            m("span", m.trust("&times"))
                        )
                    ]),
                    m("div", { class: "modal-body" }, [
                        m("div", {
                            class: "alert alert-secondary", role: "alert"
                        }, m.trust(service.elusive.description)),
                        m("table", { class: "table" }, [
                            m("tr", [m("th", "Name"), m("td", service.elusive.name)]),
                            m("tr", [m("th", "Location"), m("td", service.elusive.location)]),
                            m("tr", [m("th", "Start date"), m("td", start.format(dateFormat))]),
                            m("tr", [m("th", "End date"), m("td", end.format(dateFormat))]),
                            m("tr", [m("th", "Duration"), m("td", end.diff(start, 'days') + ' days')]),
                            m("tr", [m("th", (moment().isAfter(start)) ? 'Started since' : 'Start in'), m("td", duration.humanize())]),
                        ])
                    ]),
                    m("div", { class: "modal-footer" },
                        m("button", { class: "btn btn-info", "data-dismiss":"modal" }, "Close")
                    )
                ])
            )
        )
    }
}

function setMaintenanceModal(service) {
    if (service.nextWindow && service.ref) {
        var modalId = service.ref + "_maintenance";
        var start = moment(service.nextWindow.start);
        var end = moment(service.nextWindow.end);
        var duration = moment.duration(start.diff(moment()));
        return m("div", { class: "modal fade", id: modalId, role: "dialog" },
            m("div", { class: "modal-dialog modal-dialog-centered", role: "document" },
                m("div", { class: "modal-content" }, [
                    m("div", { class: "modal-header" }, [
                        m("h5", { class: "modal-title" }, (moment().isAfter(start)) ? "Maintenance in progress" : "Scheduled maintenance"),
                        m("button", { class: "close", type: "button", "data-dismiss": "modal" },
                            m("span", m.trust("&times"))
                        )
                    ]),
                    m("div", { class: "modal-body" }, [
                        m("div", {
                            class: "alert alert-info text-center", role: "alert"
                        }, "All online features are unavailable during maintenance. However, the game remains playable in offline mode."),
                        m("table", { class: "table" }, [
                            m("tr", [m("th", (moment().isAfter(start)) ? 'Started since' : 'Start in'), m("td", duration.humanize())]),
                            m("tr", [m("th", "Duration"), m("td", end.diff(start, 'hours') + ' hours')]),
                            m("tr", [m("th", "Start date"), m("td", start.format(dateFormat))]),
                            m("tr", [m("th", "End date"), m("td", end.format(dateFormat))])
                            
                        ])
                    ]),
                    m("div", { class: "modal-footer" },
                        m("button", { class: "btn btn-info", "data-dismiss": "modal" }, "Close")
                    )
                ])
            )
        )
    }
}

function reportService(e) {
    var ref = e.target.getAttribute('data-service-ref');
    $("#spinner-" + ref).html('<span class="spinner-grow spinner-grow-sm text-secondary" role="status"></span>');

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            triggerReport(e, ref, position.coords.latitude, position.coords.longitude);
        }, function () {
            triggerReport(e, ref, 0.0, 0.0);
        });
    } else {
        triggerReport(e, ref, 0.0, 0.0);
    }
}

function triggerReport(e, ref, latitude, longitude) {

    var name = e.target.getAttribute('data-service-name');
    var status = e.target.getAttribute('data-service-state');
    var token = $("input[name='__RequestVerificationToken']").val();
    var recaptchaKey = document.getElementById('scriptsTag').getAttribute("data-recaptcha-key");

    grecaptcha.ready(function () {
        grecaptcha.execute(recaptchaKey, { action: 'UserReport' }).then(function (recaptchaToken) {
            FingerprintJS.load().then(fp => {
                fp.get().then(result => {
                    var murmur = result.visitorId;

                    if (!murmur || 32 !== murmur.length) {
                        browserNotCompatible("Invalid hash.", ref);
                        return;
                    }

                    if (!token || 0 === token.length) {
                        browserNotCompatible("Missing form token.", ref);
                        return;
                    }

                    if (!recaptchaToken || 0 === recaptchaToken.length) {
                        browserNotCompatible("Missing recaptcha token.", ref);
                        return;
                    }

                    $.ajax({
                        type: "post",
                        url: "/UserReports/SubmitReport",
                        data: {
                            reference: ref,
                            fingerprint: murmur,
                            state: status,
                            recaptchaToken: recaptchaToken,
                            latitude: latitude,
                            longitude: longitude,
                            __RequestVerificationToken: token
                        },
                        success: function (data) {
                            if (data.type == "success") {
                                showNotification("success", "Your report has been saved.", "Platform : " + name);
                                $("#spinner-" + ref)
                                    .html("&#10003")
                                    .css("color", "#61b329");
                            } else if (data.type == "info") {
                                showNotification("info", null, data.message);
                                $("#spinner-" + ref)
                                    .html("&times")
                                    .css("color", "#ff6a00");
                            } else if (data.type == "warning") {
                                showNotification("warning", null, data.message);
                                $("#spinner-" + ref)
                                    .html("&times")
                                    .css("color", "#ff6a00");
                            } else if (data.type == "error") {
                                showNotification("error", null, data.message);
                                $("#spinner-" + ref)
                                    .html("&times")
                                    .css("color", "#c00");
                            }
                        },
                        error: function () {
                            showNotification("error", null, "An error has occurred.");
                            $("#spinner-" + ref)
                                .html("&times")
                                .css("color", "#c00");
                        }
                    });
                });
            });
        });
    });
}

function browserNotCompatible(errorMessage, ref) {
    showNotification("error", null, "Your browser is not compatible with this operation : " + errorMessage);
    $("#spinner-" + ref)
        .html("&times")
        .css("color", "#ff6a00");
}

m.mount(servicesView, services);