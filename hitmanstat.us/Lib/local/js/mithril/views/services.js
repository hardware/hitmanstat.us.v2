var services = services || {};
var dateFormat = 'YYYY.MM.DD hh:mmA';

services.oncreate = function () {
    window.addEventListener("load", function () {
        services.refresh();
        services.renderChart();
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
                        }, "Last updated : " + moment(service.lastCheck).fromNow()),
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
    if (service.name.indexOf("pc") !== -1) {
        return '<i class="fab fa-steam"></i><br />' + service.name;
    } else if (service.name.indexOf("xbox") !== -1) {
        return '<i class="fab fa-xbox"></i><br />' + service.name;
    } else if (service.name.indexOf("ps4") !== -1) {
        return '<i class="fab fa-playstation"></i><br />' + service.name;
    } else if (service.name.indexOf("auth") !== -1) {
        return '<i class="fas fa-user-lock"></i> ' + service.name;
    } else if (service.name.indexOf("forum") !== -1) {
        return '<i class="fab fa-discourse"></i> ' + service.name;
    }
}

function setLink(service) {
    if (service.url) {
        return m("a", {
            class: "btn btn-outline-secondary btn-sm float-right", href: service.url
        }, "Website");
    }
}

function setReport(service) {
    if (service.group != "ot") {
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
            Fingerprint2.get(function (components) {
                var values = components.map(function (component) { return component.value });
                var murmur = Fingerprint2.x64hash128(values.join(''), 31);

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
}

function browserNotCompatible(errorMessage, ref) {
    showNotification("error", null, "Your browser is not compatible with this operation : " + errorMessage);
    $("#spinner-" + ref)
        .html("&times")
        .css("color", "#ff6a00");
}

m.mount(servicesView, services);