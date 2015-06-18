var musicHubProxy = $.connection.musicHub;
var deviceId;

musicHubProxy.client.setState = function (deviceState) {
    console.log(deviceState);
}

$.connection.hub.start();

$('#connect').click(function(ev) {
    deviceId = $('#deviceId').val();
    musicHubProxy.invoke('ConnectToDevice', deviceId);
}.bind(this));

$('#queue').click(function(ev) {
    var trackIds = $('#queueItems').val().split(",").map(function (id) { return id.trim() });
    musicHubProxy.invoke('Queue', deviceId, trackIds);
});

$('#play').click(function(ev) {
    musicHubProxy.invoke('Play', deviceId);
});

$('#next').click(function (ev) {
    musicHubProxy.invoke('RequestNext', deviceId);
});

$('#pause').click(function (ev) {
    musicHubProxy.invoke('Pause', deviceId);
});