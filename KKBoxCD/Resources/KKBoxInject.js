challenge = function() {
    const country_code = "65";
    const territory_code = "SG";

    srp = new SRP();
    srp.init();
    srp.computeA();
    
    $("#username").val(window.__username);
    $("#ori_username").val(window.__username);
    $("#secret").val(window.__password);
    $("#remember").val("");
    $("#phone_country_code").val(country_code);
    $("#phone_territory_code").val(territory_code);

    $.ajax({
        method: "POST",
        url: "/challenge",
        data: {
            username: $("#username").val(),
            phone_country_code: country_code,
            phone_territory_code: territory_code,
            a: srp.A.toString(16),
        },
    }).fail(function(xhr) {
        switch (xhr.status) {
            case 401:
                toast("not_verified");
                break;
            case 403:
                $("#t").val(xhr.responseJSON.data.t);
                $("#form-login").submit();
                break;
            case 404:
                toast("login_failed");
                break;
            case 500:
                toast("server_error");
                break;
            default:
                toast("reload");
        }
    }).done(function(data) {
        challenge_reply(data);
    });
}

challenge_reply = function(data) {
    $("#username").val(data.q);
    srp.init(data.g);
    g = data.g
    srp.s = data.s;
    srp.computeA();

    $.ajax({
        method: "POST",
        url: "/challenge_verify",
        data: {
            username: $("#username").val(),
            a: srp.A.toString(16),
        },
    }).fail(function(xhr) {
        switch (xhr.status) {
            case 400:
            case 404:
                toast("login_failed");
                break;
            case 500:
                toast("reload");
                break;
            default:
                toast("reload");
        }
    }).done(function(data) {
        operation_user_protection(data);
    });
}

operation_user_protection = function(data) {
    srp.I = $("#username").val();
    srp.p = srp.computeHash($("#secret").val());
    srp.B = new BigInteger(data.B, 16);
    if (g === "G2048") {} else {
        $("#form-login").submit();
        return
    }
    if (!srp.verifyB() && srp.verifyHAB()) {
        challenge();
        return;
    }
    srp.computeVerifier();
    let ck = srp.computeClientK();
    $("#secret").val(ck);
    $("#form-login").submit();
}

toast = function(content) {
    if (window.__toast == null) {
        window.__toast = document.createElement("h1");
        window.__toast.id = "toast-content";
        document.body.insertBefore(window.__toast, document.body.firstChild);   
    }
    window.__toast.innerText = content;
};