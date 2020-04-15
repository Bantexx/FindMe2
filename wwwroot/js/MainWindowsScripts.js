$(function () {
    let isExecuted = false;
    $(".link__proff").click(function () {
        if (!isExecuted) {
            $(".hide_block").css('visibility','visible');
            isExecuted = true;
        }   
    });
});