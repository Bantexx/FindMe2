const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();
hubConnection.serverTimeoutInMilliseconds = 1000 * 60 * 10;
var form = document.getElementById("send_mess");
var input = document.getElementById("messengerChat");

function ScrollToLastMess() {
    var messages = document.getElementsByClassName('chat__text');
    var last_message = messages[messages.length - 1];
    $('.messenger__chat').animate({
        scrollTop: $(last_message).offset().top
    }, 1);
}

hubConnection.on('ShowMess', function (message, to,mess_time) {
    var chat = document.getElementsByClassName("messenger__chat")[0];
    if (input[0] !== null) {  
        $.ajax({
            url: '/Home/SendMessageByUser',
            type: 'POST',
            data: { mess: message, date: mess_time, idSender: to },
            dataType: 'text',
            success: function (data) {
                chat.insertAdjacentHTML("beforeend", data);
            }
        });
    }
});

if (form !== null) {
    form.addEventListener("submit", function (event) {
        if (input.value === '') {
            event.preventDefault();
        } else {
            var currenttime = new Date();
            var sendtime = currenttime.toLocaleString();
            $.ajax({
                url: '/Home/SendMessageByUser',
                type: 'POST',
                data: { mess: input.value, date: sendtime },
                dataType: 'text',
                success: function (data) {
                    var chat = document.getElementsByClassName("messenger__chat")[0];
                    chat.insertAdjacentHTML("beforeend", data);
                }
            });
            var receiver = document.getElementsByClassName("messenger__name")[0].id;
            var chat_id = document.getElementsByClassName("messenger__header")[0].id
            hubConnection.invoke("Send", input.value, receiver, chat_id, sendtime);
            input.value = null;
            event.preventDefault();
        }
    }, false);
}
hubConnection.start();
$('#usersSearch').submit(function (e) {
    let input = document.getElementById('SearchUserChat').value;
    if (input === '') {
        e.preventDefault();
    }
});
$('.messenger__chat').ready(function (e) {
    ScrollToLastMess();  
});

