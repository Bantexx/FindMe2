var open_add_news = document.getElementsByClassName('tags__add-link')[0];
let star = document.getElementsByClassName('active');
var lent_profiles = document.getElementsByClassName('card__header');
var openPopupTags = document.getElementsByClassName('news-popup__add-tags')[0];

if (star.length > 0) {
    [].forEach.call(star, function (elem) {
        elem.addEventListener('click', function (e) {
            let id_news = this.parentNode.parentNode.id;
            if (this.classList.contains('icon-active')) {
                $.ajax({
                    url: '/Home/DeleteFromFavoritesNews',
                    type: 'POST',
                    async:false,
                    data: { idn: id_news },
                    error: function () {
                        e.preventDefault();
                    }
                });
                this.classList.remove('icon-active');
            } else {
                $.ajax({
                    url: '/Home/AddToFavoriteNews',
                    type: 'POST',
                    async: false,
                    data: { idn: id_news },
                    error: function () {
                        e.preventDefault();
                    }
                });
                this.classList.add('icon-active');              
            };
            updateNewsInterested(id_news, function (data) {
                e.target.parentNode.children[2].innerHTML = data;
            });
        });
    });
};

if (open_add_news !== null && !!open_add_news) {
    open_add_news.addEventListener('click', function (e) {
        let modalID = e.target.getAttribute('data-id');
        document.getElementById(modalID).style.display = 'block';

        let close_add_news = document.getElementsByClassName('news-popup__close')[0];
        close_add_news.addEventListener('click', function (e) {
            var popup = document.getElementById('popupAdd');
            popup.style.display = 'none';
        });
    });
};
if (lent_profiles.length>0) {
    var main_lent = document.getElementsByClassName('content')[0];
    var check_popups = document.getElementsByClassName('people-popup');
    
    main_lent.addEventListener('click', function () {
        if (check_popups.length > 0) {
            check_popups[0].remove();
        }
    });

    [].forEach.call(lent_profiles, function (el) {
        el.addEventListener('click', function (e) {
            if (check_popups.length > 0) {
                check_popups[0].remove();
            }
            $.ajax({
                url: '/Home/ShowProfilePopup',
                type: 'POST',
                data: { id_user: this.getAttribute('data_id') },
                dataType: 'html',
                success: function (data) {
                    el.parentNode.insertAdjacentHTML('beforeEnd', data);
                }
            });
        });
    });
}
   

openPopupTags.addEventListener('click', function (e) {
    var openTagsAdd = document.getElementsByClassName('popup-tags__blocks')[0];
    var closePopupNews = document.getElementsByClassName('news-popup__inner')[0];
    openTagsAdd.style.display = 'flex';
    closePopupNews.style.display = 'none';
    var closePopupTags = document.getElementsByClassName('popup__close')[0];
    closePopupTags.addEventListener('click', function (e) {
        openTagsAdd.style.display = 'none';
        closePopupNews.style.display = 'block';
    });
    var addTag = document.getElementsByClassName('popup');
    [].forEach.call(addTag, function (elem) {
        elem.addEventListener('click', function (e) {
            this.classList.toggle('popup-active');
            this.childNodes[1].classList.toggle('popup__icon-active');
        });
    });
});

function updateNewsInterested(idn,call_fn) {
    $.ajax({
        url: '/Home/GetInterestedNews',
        type: 'POST',
        async: false,
        data: { id_news: idn },
        success: function (e) {
            call_fn(e);
        }
    });
};

$('.cards').ready(function () {
    $('.check_masonry').masonry({
        itemSelector: '.card',
        singleMode: true,
        isResizable: true,
        isAnimated: true,
        animationOptions: {
            queue: false,
            duration: 500
        }
    });
});

    

