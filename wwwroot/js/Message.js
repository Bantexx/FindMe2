var send_mess_form = document.getElementById("send_mess");
const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chat")
    .build();
hubConnection.serverTimeoutInMilliseconds = 1000 * 60 * 10;
hubConnection.on('ShowMess', function (chat_id) {
    var chat = document.getElementsByClassName("messenger__chat")[0];
    if (input_mess[0] !== null) {  
        $.ajax({
            url: '/Home/ShowMessage',
            type: 'POST',
            data: { idc: chat_id },
            dataType: 'text',
            success: function (data) {
                chat.insertAdjacentHTML("beforeend", data);
                ScrollToLastMess();
            }
        });
    }
});
hubConnection.start();
if (!!send_mess_form && send_mess_form !== null) {
    send_mess_form.addEventListener("submit", function (event) {
        if (input_mess.value === '' && attach_files.files.length === 0) {
            event.preventDefault();
        };
    }, false);
};



