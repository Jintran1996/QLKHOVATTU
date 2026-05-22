//cookies
function setCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}
function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}
function eraseCookie(name) {
    document.cookie = name + '=; Max-Age=-99999999;';
}
//end

//notify
function notify(messenge, type) {
    switch (type) {
        case 'success':
            toastr.success(messenge);
            break;
        case 'info':
            toastr.info(messenge);
            break;
        case 'error':
            toastr.error(messenge);
            break;
        case 'warning':
            toastr.warning(messenge);
            break;
        default:
            toastr.error(messenge);
    }
}
function notifyClose(messenge, type) {
    switch (type) {
        case 'success':
            $(document).Toasts('create', {
                class: 'bg-success',
                title: 'Success',
                subtitle: '',
                body: messenge,
                autohide: true,
                delay: 5000
            })
            break;
        case 'info':
            $(document).Toasts('create', {
                class: 'bg-info',
                title: 'Info',
                subtitle: '',
                body: messenge,
                autohide: true,
                delay: 5000
            })
            break;
            break;
        case 'error':
            $(document).Toasts('create', {
                class: 'bg-danger',
                title: 'Error',
                subtitle: '',
                body: messenge,
                autohide: true,
                delay: 5000
            })
            break;
        case 'warning':
            $(document).Toasts('create', {
                class: 'bg-warning',
                title: 'Warning',
                subtitle: '',
                body: messenge,
                autohide: true,
                delay: 5000
            })
            break;
        default:
            $(document).Toasts('create', {
                class: 'bg-danger',
                title: 'Error',
                subtitle: '',
                body: messenge,
                autohide: true,
                delay: 5000
            })
    }
}
//end

$("#openMenu").click(function () {
    if (typeof window.orientation == 'undefined') {
        if (getCookie('openMenu') != null) {
            if (getCookie('openMenu') == 'close') {
                console.log('open');
                $('#bodyApp').attr('class', 'sidebar-mini layout-fixed layout-navbar-fixed layout-footer-fixed text-sm sidebar-collapse');
                setCookie('openMenu', 'open', 365);
            } else {
                console.log('close');
                $('#bodyApp').attr('class', 'sidebar-mini layout-fixed layout-navbar-fixed layout-footer-fixed text-sm');
                setCookie('openMenu', 'close', 365);
            }
        } else {
            console.log('null');
            $('#bodyApp').attr('class', 'sidebar-mini layout-fixed layout-navbar-fixed layout-footer-fixed text-sm sidebar-collapse');
            setCookie('openMenu', 'close', 365);
        }
    }
});

$(document).ready(function () {
    if (typeof window.orientation == 'undefined') {
        if (getCookie('openMenu') != null) {
            if (getCookie('openMenu') == 'close') {
                $('#bodyApp').attr('class', 'sidebar-mini layout-fixed layout-navbar-fixed layout-footer-fixed text-sm sidebar-collapse');
            } else {
                $('#bodyApp').attr('class', 'sidebar-mini layout-fixed layout-navbar-fixed layout-footer-fixed text-sm');

            }
        } else {
            $('#bodyApp').attr('class', 'sidebar-mini layout-fixed layout-navbar-fixed layout-footer-fixed text-sm sidebar-collapse');
            setCookie('openMenu', 'close', 365);
        }
    }
});

//login/index
$('#fLogin_cRemember').change(function () {
    $('#fLogin_cRemember').val($(this).is(':checked') ? "True" : "False");
});
function funfLogin_bLogin(url) {
    form = $("#fLogin");
    form.removeData('validator');
    form.removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse("#fLogin");
    if (form.valid()) {
        var formdata = new FormData(); //FormData object
        formdata.append("ReturnUrl", $("#ReturnUrl").val());
        formdata.append("Username", $("#fLogin_tUsername").val());
        formdata.append("Password", $("#fLogin_pPassword").val());
        formdata.append("Remember", $("#fLogin_cRemember").val());
        $.ajax({
            url: url,
            type: "POST",
            data: formdata,
            contentType: false,
            processData: false,
            beforeSend: function () {
                $(".divLoading").addClass("loading");
            },
            success: function (result) {
                if (result.status == 1) {

                    notify(result.text, "success");

                    window.location.href = result.obj;
                }
                else {
                    notify(result.text, "error");
                }

                $(".divLoading").removeClass("loading");
            },
            error: function (message) {
                $(".divLoading").removeClass("loading");
            }
        });
    }
}
//end

var config_search = {
    '.chosen-select-search': {},
    '.chosen-select-deselect': { allow_single_deselect: true },
    '.chosen-select-no-single': { disable_search_threshold: 10 },
    '.chosen-select-no-results': { no_results_text: 'Oops, nothing found!' },
    '.chosen-select-width': { width: "95%" },
    'search_contains': true
}
for (var selector in config_search) {
    $(selector).chosen(config_search);
}
