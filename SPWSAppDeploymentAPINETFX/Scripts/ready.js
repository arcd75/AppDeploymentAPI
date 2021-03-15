$(document).ready(function () {
    console.log("ready function");
    $.support.cors = !0;
   
    adHub = $.connection.aDHub;
    adHub.client.updateNetClients = function () {
        RefreshClients();
    };
    $.connection.hub.start().done(function () {
        adHub.server.webJoin(location.hostname);
    });
    

    viewModel = new ViewModel();
    ko.applyBindings(viewModel);
    RefreshClients();
});