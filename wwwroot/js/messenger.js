var open_btn = document.getElementsByClassName('btn__messenger')[0];
var open_mess = document.getElementsByClassName('messenger__hookup')[0];
var open_dots = document.getElementsByClassName('chat__icon');
var input_mess = document.getElementById("messengerChat");
var delete_chats = document.getElementsByClassName('users__name-close');
var attach_files = document.getElementById('attachFiles');
let codes_emoji = ['&#128514;', '&#128513;', '&#128517;', '&#128512;', '&#128513;', '&#128514;', '&#129315;', '&#128515;', '&#128516;', '&#128517;', '&#128518;', '&#128521;', '&#128522;', '&#128523;', '&#128526;', '&#128525;', '&#128536;', '&#129392;',
    '&#128535;', '&#128537;', '&#128538;', '&#128578;', '&#129303;', '&#129321;', '&#129300;', '&#129320;', '&#128528;', '&#128529;', '&#128566;', '&#128580;', '&#128527;', '&#128547;', '&#128549;', '&#128558;', '&#129296;', '&#128559;', '&#128554;',
    '&#128555;', '&#128564;', '&#128524;', '&#128539;', '&#128540;', '&#128541;', '&#129316;', '&#128530;', '&#128531;', '&#128532;', '&#128533;', '&#128579;', '&#129297;', '&#128562;', '&#128577;', '&#128534;', '&#128542;', '&#128543;', '&#128548;',
    '&#128546;', '&#128557;', '&#128550;', '&#128551;', '&#128552;', '&#128553;', '&#129327;', '&#128556;', '&#128560;', '&#128561;', '&#129397;', '&#129398;', '&#128563;', '&#129322;', '&#128565;', '&#128545;', '&#128544;', '&#129324;', '&#128567;',
    '&#129298;', '&#129301;', '&#129314;', '&#129326;', '&#129319;', '&#128519;', '&#129312;', '&#129313;', '&#129395;', '&#129396;', '&#129402;', '&#129317;', '&#129323;', '&#129325;', '&#129488;', '&#129299;', '&#128520;', '&#128127;', '&#128121;',
    '&#128122;', '&#128128;', '&#128123;', '&#128125;', '&#129302;', '&#128169;', '&#128570;', '&#128568;', '&#128569;', '&#128571;', '&#128572;', '&#128573;', '&#128576;', '&#128575;', '&#128574;'];



if (delete_chats.length>0) {
    [].forEach.call(delete_chats, function (elem) {
        elem.addEventListener('click', function (e) {
            let chat_id = this.parentNode.getAttribute('data_id');
            $.ajax({
                url: '/Home/DeleteCurrentChat',
                type: 'POST',
                data: { id_c: chat_id },
                success: function (data) {
                    if (data == true) {
                        elem.parentNode.remove();
                    };
                }
            });
        });
    });
}

if (!!open_btn) {
    open_btn.addEventListener('click', function (e) {
        var input = document.getElementsByClassName('messenger__search')[0];
        var popup_mess = document.getElementById('messPopup');
        input.style.display = 'block';
        popup_mess.style.right = '485px';
        open_btn.style.display = 'none';
        var inputClose = document.getElementById('inputClose');
        inputClose.addEventListener('click', function (e) {
            popup_mess.style.right = '38px';
            input.style.display = 'none';
            open_btn.style.display = 'inline-block';
        })
    });
};

if (!!open_mess) {
    open_mess.addEventListener('click', function (e) {
        var popup_mess = document.getElementById('messPopup');
        popup_mess.style.display = 'block';
        e.stopPropagation();
        var messClose = document.getElementsByClassName('btn__messenger')[0];
        messClose.addEventListener('click', function (e) {
            popup_mess.style.display = 'none';
        });
    });
};

if (open_dots.length > 0) {
    showDots();
};

$('#usersSearch').submit(function (e) {
    let input = document.getElementById('SearchUserChat').value;
    if (input === '') {
        e.preventDefault();
    }
});

$('.messenger__chat').ready(function () {
    ScrollToLastMess();
    attach_files.addEventListener('change', function () {
        let files = this.files;
        let block_images = document.getElementsByClassName('messenger__footer-img')[0];
        for (var f of files) {
            var reader = new FileReader();
            reader.onload = function (elem) {
                var new_img = '<picture class="messenger__footer-img"><img src=' + elem.target.result + '><div class="messenger__footer-icon"></div></picture>';
                block_images.insertAdjacentHTML('beforeend', new_img);
            }
            reader.readAsDataURL(f);
        }
    });
    let list_emoji = document.getElementsByClassName('emoji-popup__list')[0];
    for (let cod of codes_emoji) {
        list_emoji.insertAdjacentHTML('beforeEnd', "<li class='emoji-popup__item'>" +
            "<a class='emoji-popup__link' href='#'>" +
            "<span class='emoji-popup__img'>" + cod + "</span></a>" +
            "</li>"
        );
    };
    let openEmojiPopup = document.getElementsByClassName('btn__chat')[0];
    openEmojiPopup.addEventListener('click', function (e) {
        let emojiPopup = document.getElementsByClassName('emoji-popup')[0];
        emojiPopup.style.display = 'block';
        e.stopPropagation();
    });
    let emojis = document.getElementsByClassName('emoji-popup__img');
    [].forEach.call(emojis, function (elem) {
        elem.addEventListener('click', function (e) {
            input_mess.value += e.target.innerHTML;
            e.stopPropagation();
        });
    });
});

$('#inputBtn').click(function (e) {
    var input_search = document.getElementById('messengerSearch').value;
    if (input_search === '') {
        e.preventDefault();
    }
});

/*$('#inputBtn').click(function (e) {
    var input_search = document.getElementById('messengerSearch').value;
    if (input_search !== '') {
        var text_messages = document.getElementsByClassName('chat__text');
        if (text_messages.length > 0) {
            for (let i = 0; i < text_messages.length; i++) {
                if (text_messages[i].innerHTML.indexOf(input_search) === -1) {
                    text_messages[i].parentNode.parentNode.remove();
                    i--;
                };
            }
        }
    } else {
        e.preventDefault();
    }
   
});*/

function ScrollToLastMess() {
    var messages = document.getElementsByClassName('messenger__chat')[0];
    messages.scrollTop = messages.scrollHeight;
};
function CloseHoverMenu() {
    var popup_menu = document.getElementsByClassName('hover-menu');
    [].forEach.call(popup_menu, function (elem) {
        elem.remove();
    });
};
function CheckIsMessFavo(chat_id,call_fn) {
    $.ajax({
        url: '/Home/GetFavoMess',
        type: 'POST',
        data: { id_c: chat_id },
        success: function (data) {
            call_fn(data);
        }
    });
}
function showDots() {
    let current_chat_id = document.getElementsByClassName('messenger__header')[0].id;
    let fav_messes = [];
    CheckIsMessFavo(current_chat_id, function (data) {
        fav_messes = data;
    });
    [].forEach.call(open_dots, function (elem) {
        elem.addEventListener('click', function (e) {
            CloseHoverMenu();
            let id_mess = e.target.parentNode.getAttribute('data_id');
            if (isNaN(parseInt(id_mess))) {
                return false;
            }
            let hover_menu;
            if (fav_messes.includes(Number(id_mess))) {
                hover_menu =
                    "<div class='hover-menu' data_id='hoverMenu'>" +
                    "<ul class='hover-menu__item'>" +
                    "<li class='hover-menu__list' ><a href='' id='DelFromFavor' class='hover-menu__link'>Убрать из избранных</a></li>" +
                    "<li class='hover-menu__list'><a href='' id='DelCurrMess' class='hover-menu__link'>Удалить</a></li>" +
                    "<li class='hover-menu__list'><a href='' id='MarkAsRead' class='hover-menu__link'>Отметить как не прочитанное</a></li>" +
                    "</ul >" +
                    "</div>";
                this.insertAdjacentHTML('afterend', hover_menu);
                $('#DelFromFavor').click(function () {
                    $.ajax({
                        url: '/Home/DelFromFavMess',
                        type: 'POST',
                        data: { id_c: current_chat_id, id_m: id_mess },
                        success: function (data) {

                        }
                    });
                });
            } else {
                hover_menu =
                    "<div class='hover-menu' data_id='hoverMenu'>" +
                    "<ul class='hover-menu__item'>" +
                    "<li class='hover-menu__list' ><a href='' id='AddtoFavor' class='hover-menu__link'>Добавить в избранное</a></li>" +
                    "<li class='hover-menu__list'><a href='' id='DelCurrMess' class='hover-menu__link'>Удалить</a></li>" +
                    "<li class='hover-menu__list'><a href='' id='MarkAsRead' class='hover-menu__link'>Отметить как не прочитанное</a></li>" +
                    "</ul >" +
                    "</div>";
                this.insertAdjacentHTML('afterend', hover_menu);
                $('#AddtoFavor').click(function () {
                    $.ajax({
                        url: '/Home/AddToFavMess',
                        type: 'POST',
                        data: { id_c: current_chat_id, id_m: id_mess },
                        success: function (data) {

                        }
                    });
                });
            }
            $('#DelCurrMess').click(function () {
                $.ajax({
                    url: '/Home/DelMessFromChat',
                    type: 'POST',
                    data: { id_c: current_chat_id, id_m: id_mess },
                    success: function (data) {
                        e.target.parentNode.parentNode.remove();
                    }
                });
            });
            e.stopPropagation();
        });
    });
    document.getElementsByClassName('messenger')[0].addEventListener('click', function () {
        var popup_mess = document.getElementById('messPopup');
        let emojiPopup = document.getElementsByClassName('emoji-popup')[0];
        if (popup_mess.style.display === 'block') {
            popup_mess.style.display = 'none';
        }
        if (emojiPopup.style.display === 'block') {
            emojiPopup.style.display = 'none';
        }
        CloseHoverMenu();
    });
};