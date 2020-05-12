let search_input = document.getElementById("headerSearch");
let search_form = document.getElementById("search_form");
let head_location = document.getElementsByClassName("header__location")[0];

head_location.addEventListener("click", function () {
    getUserLocation();
}, false);

search_form.addEventListener("submit", function (event) {
    if (search_input.value === '') {
        event.preventDefault();
    }
}, false);

document.addEventListener("DOMContentLoaded",function () {
    getUserLocation();
});

function getUserLocation() {
    let ip_adr;
    $.get('https://www.cloudflare.com/cdn-cgi/trace', function (data) {
        ip_adr = data.substr(data.indexOf("ip=") + 3, 14);
    });
    $.ajax({
        url: '/Home/GetLocation',
        type: 'POST',
        data: ip_adr,
        dataType: 'html',
        success: function (data) {
            $('.location__link').html(data);
        }
    });
}






