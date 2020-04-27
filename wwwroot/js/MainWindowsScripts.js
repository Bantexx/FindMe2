var search_input = document.getElementById("headerSearch");
var search_form = document.getElementById("search_form");
var open = document.getElementById('add_tags');

if (open !== null) {
    open.addEventListener('click', function (e) {
        var tags = document.getElementsByClassName("general__inner")[0];
        $.ajax({
            url: '/Home/ShowPopUpTags',
            type: 'POST',
            data: '',
            dataType: 'html',
            success: function (data) {
                tags.insertAdjacentHTML("beforeend", data);
                var close = document.getElementById('close__link');
                var mytags = document.getElementsByClassName('user__tags-popup')[0];
                var poptags = document.getElementsByClassName('popular__tags-popup')[0];
                close.addEventListener('click', function () {
                    var popup = document.getElementById('popupProff');
                    popup.parentNode.removeChild(popup);
                });
                document.getElementById('my_tags_id').addEventListener('click', function (e) {
                    mytags.style.visibility = 'visible';
                    poptags.style.visibility = 'hidden';
                    e.preventDefault();               
                });
                document.getElementById('pop_tags_id').addEventListener('click', function (e) {
                    mytags.style.visibility = 'hidden';
                    poptags.style.visibility = 'visible';
                    e.preventDefault();
                });
                $(".user__link-sub").click(function (e) {
                    $.ajax({
                        url: '/Home/DelUserTag',
                        type: 'POST',
                        data: { id_tag:$(this).attr('id') }
                    });
                    $(this).remove();
                    e.preventDefault();
                });
                $(".pop__link-sub").click(function (e) {
                    $.ajax({
                        url: '/Home/AddTagToUser',
                        type: 'POST',
                        data: { id_tag: $(this).attr('id') }
                    });
                    e.preventDefault();
                });
                
            }
        });
    });
}

$(function () {
    let isExecuted = false;
    $(".link__proff").click(function () {
        if (!isExecuted) {
            $(".hide_block").css('visibility','visible');
            isExecuted = true;
        }   
    });
});

$(".icon__favorites").click(function () {
    $(this).css('content', 'url(/img/cards/yellow_star.png)');
    //alert($(this).parents(".card").attr('id'));
});

$(".header__location").click(function () {
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
});
search_form.addEventListener("submit", function (event) {
    if (search_input.value === '') {
        event.preventDefault();
    }
}, false);



