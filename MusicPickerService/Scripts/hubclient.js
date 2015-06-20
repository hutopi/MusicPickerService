var musicHubProxy = $.connection.musicHub;
var deviceId;

musicHubProxy.client.setState = function (deviceState) {
    console.log(deviceState);
}

musicHubProxy.client.setVoteOptions = function(options) {
    console.log(options);
}

$('#connect').click(function(ev) {
    deviceId = $('#deviceId').val();
    $.connection.hub.qs = { 'access_token': $('#bearer').val() };
    $.connection.hub.start().done(function () {
        musicHubProxy.invoke('RegisterClient', deviceId);
    });
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

$('#submitVoteOptions').click(function(ev) {
    var trackIds = $('#voteOptions').val().split(",").map(function (id) { return id.trim() });
    musicHubProxy.invoke('SubmitVoteOptions', deviceId, trackIds);
});

$('#submitVote').click(function (ev) {
    musicHubProxy.invoke('VoteForTrack', deviceId, $('#vote').val());
});