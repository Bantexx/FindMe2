var open_popup_profile = document.getElementsByClassName('tags__title')[0];
var changeAvatar = document.getElementsByClassName('link__proff')[0];

if (changeAvatar !== null && !!changeAvatar) {
    changeAvatar.addEventListener('click', function (e) {
        var popupActive = document.getElementsByClassName('active-popup')[0];
        var popupClose = document.getElementsByClassName('active-popup__close')[0];
        var showAvatar = popupActive.getElementsByClassName('showAva')[0];
        var showSendBtn = document.getElementById('send_input');
        var label_input = document.getElementById('for_file_input');

        popupActive.style.display = 'block';
        popupClose.addEventListener('click', function (e) {
            popupActive.style.display = 'none';
            label_input.style.visibility = 'visible';
            showSendBtn.style.visibility = 'hidden';
            showAvatar.style.display = 'none';
            showAvatar.firstElementChild.src = ''; 
        });
        var changeAva = document.getElementById('file_input');
        changeAva.addEventListener('change', function (e) {
            label_input.style.visibility = 'hidden';
            showSendBtn.style.visibility = 'visible';
            showAvatar.style.display = 'block';
            showAvatar.firstElementChild.src = URL.createObjectURL(e.target.files[0]);      
        });
    });
}
if (open_popup_profile !== null && !!open_popup_profile) {
    open_popup_profile.addEventListener('click', function (e) {
        var tags = document.getElementsByClassName("proff")[0];
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
                        data: { id_tag: $(this).attr('id') }
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