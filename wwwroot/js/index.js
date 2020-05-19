var connection = new signalR.HubConnectionBuilder().withUrl("/stockQuoteHub").build();

connection.start().then(function () {
    })
    .catch(function (err) {
        return console.error(err.toString());
    });

connection.on("ReceiveQuote", function (stockInfo) {
    $("#companyName").val(stockInfo.data.companyName);
    $("#exchange").val(stockInfo.data.exchange);
    $("#quote").val(stockInfo.data.primaryData.lastSalePrice);
});

$("#subscribe").click(function () {
    var symbol = $("#symbol").val();
    connection.invoke("SubscribeSymbol", symbol).catch(function (err) {
        return console.error(err.toString());
    });
})