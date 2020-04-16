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
});

